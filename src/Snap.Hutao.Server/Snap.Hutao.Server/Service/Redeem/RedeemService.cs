// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Service.Expire;

namespace Snap.Hutao.Server.Service.Redeem;

// Scoped
public sealed class RedeemService
{
    private static readonly AsyncLock RedeemLock = new();
    private readonly GachaLogExpireService gachaLogExpireService;
    private readonly CdnExpireService cdnExpireService;
    private readonly AppDbContext appDbContext;

    public RedeemService(IServiceProvider serviceProvider)
    {
        gachaLogExpireService = serviceProvider.GetRequiredService<GachaLogExpireService>();
        cdnExpireService = serviceProvider.GetRequiredService<CdnExpireService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    // TODO: Database operation is not transactional
    public async Task<RedeemUseResponse> UseRedeemCodeAsync(RedeemUseRequest request)
    {
        using (await RedeemLock.LockAsync().ConfigureAwait(false))
        {
            ArgumentNullException.ThrowIfNull(request.Username);
            if (request.Code.Length is not 18)
            {
                return new(RedeemStatus.Invalid);
            }

            if (appDbContext.RedeemCodes.SingleOrDefault(c => c.Code == request.Code) is not { } code)
            {
                return new(RedeemStatus.NotExists);
            }

            // One-time logic is lifted
            if (code.Type is RedeemCodeType.OneTime)
            {
                if (code.IsUsed)
                {
                    return new(RedeemStatus.AlreadyUsed);
                }

                if (await this.ExtendTermForUserNameByCodeAsync(request.Username, code).ConfigureAwait(false) is { Kind: TermExtendResultKind.Ok } result)
                {
                    code.IsUsed = true;
                    code.UseBy = request.Username;
                    code.UseTime = DateTimeOffset.UtcNow;
                    code.UseCount++; // Although it's one-time, we still want to count it
                    await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
                    return new(RedeemStatus.Ok, code.ServiceType, result.ExpiredAt);
                }

                return new(RedeemStatus.DbError);
            }

            if (code.Type.HasFlag(RedeemCodeType.TimeLimited))
            {
                if (code.ExpireTime <= DateTimeOffset.UtcNow)
                {
                    return new(RedeemStatus.Expired);
                }
            }

            if (code.Type.HasFlag(RedeemCodeType.TimesLimited))
            {
                if (code.TimesAllowed <= code.UseCount)
                {
                    return new(RedeemStatus.NotEnough);
                }
            }

            if (await this.ExtendTermForUserNameByCodeAsync(request.Username, code).ConfigureAwait(false) is { Kind: TermExtendResultKind.Ok } result2)
            {
                code.UseCount++;
                await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
                return new(RedeemStatus.Ok, code.ServiceType, result2.ExpiredAt);
            }

            return new(RedeemStatus.DbError);
        }
    }

    public async Task<RedeemGenerateResponse> GenerateRedeemCodesAsync(RedeemGenerateRequest req)
    {
        List<string> codes = [];
        for (uint i = 0; i < req.Count; i++)
        {
            RedeemCode code = new()
            {
                Code = Guid.NewGuid().ToString().ToUpperInvariant()[..18],
                Type = req.Type,
                ServiceType = req.ServiceType,
                Value = req.Value,
                Description = req.Description,
                Creator = req.Creator,
                CreateTime = DateTimeOffset.UtcNow,
            };

            if (code.Type.HasFlag(RedeemCodeType.TimeLimited))
            {
                code.ExpireTime = req.ExpireTime;
            }

            if (code.Type.HasFlag(RedeemCodeType.TimesLimited))
            {
                code.TimesAllowed = req.Times;
            }

            // Avoid duplicate code
            if (!await appDbContext.RedeemCodes.AnyAsync(c => c.Code == code.Code).ConfigureAwait(false))
            {
                codes.Add(code.Code);
                await appDbContext.RedeemCodes.AddAndSaveAsync(code).ConfigureAwait(false);
            }
        }

        return new(codes);
    }

    private ValueTask<TermExtendResult> ExtendTermForUserNameByCodeAsync(string username, RedeemCode code)
    {
        IExpireService expireService = code.ServiceType switch
        {
            RedeemCodeTargetServiceType.GachaLog => gachaLogExpireService,
            RedeemCodeTargetServiceType.Cdn => cdnExpireService,
            _ => throw new NotSupportedException(),
        };

        return expireService.ExtendTermForUserNameAsync(username, code.Value);
    }
}