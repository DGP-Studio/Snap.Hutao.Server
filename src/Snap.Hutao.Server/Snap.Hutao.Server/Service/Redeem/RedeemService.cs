// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;
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

            await appDbContext.Entry(code).Collection(c => c.UseItems).LoadAsync().ConfigureAwait(false);

            if (code.Type.HasFlag(RedeemCodeType.TimeLimited))
            {
                if (code.ExpireTime <= DateTimeOffset.UtcNow)
                {
                    return new(RedeemStatus.Expired);
                }
            }

            if (code.Type.HasFlag(RedeemCodeType.TimesLimited))
            {
                if (code.TimesAllowed <= code.UseItems.Count)
                {
                    if (code.TimesAllowed == 1)
                    {
                        return new(RedeemStatus.AlreadyUsed);
                    }

                    return new(RedeemStatus.NotEnough);
                }
            }

            return await ExtendTermForUserNameByCodeAsync(request.Username, code).ConfigureAwait(false);
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

            // Avoid generating duplicate code
            if (!await appDbContext.RedeemCodes.AnyAsync(c => c.Code == code.Code).ConfigureAwait(false))
            {
                codes.Add(code.Code);
                await appDbContext.RedeemCodes.AddAndSaveAsync(code).ConfigureAwait(false);
            }
        }

        return new(codes);
    }

    private async ValueTask<RedeemUseResponse> ExtendTermForUserNameByCodeAsync(string username, RedeemCode code)
    {
        using (IDbContextTransaction transaction = await appDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
        {
            IExpireService expireService = code.ServiceType switch
            {
                RedeemCodeTargetServiceType.GachaLog => gachaLogExpireService,
                RedeemCodeTargetServiceType.Cdn => cdnExpireService,
                _ => throw new NotSupportedException(),
            };

            TermExtendResult result = await expireService.ExtendTermForUserNameAsync(username, code.Value).ConfigureAwait(false);
            if (result.Kind is TermExtendResultKind.NoSuchUser)
            {
                return new(RedeemStatus.NoSuchUser);
            }

            if (result.Kind is TermExtendResultKind.DbError)
            {
                return new(RedeemStatus.DbError);
            }

            RedeemCodeUseItem useItem = new()
            {
                RedeemCodeId = code.Id,
                UseBy = username,
                UseTime = DateTimeOffset.UtcNow,
            };

            await appDbContext.RedeemCodeUseItems.AddAndSaveAsync(useItem);
            await transaction.CommitAsync().ConfigureAwait(false);

            return new(RedeemStatus.Ok, code.ServiceType, result.ExpiredAt);
        }
    }
}