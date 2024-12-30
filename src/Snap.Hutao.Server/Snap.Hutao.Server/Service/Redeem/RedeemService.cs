// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Expire;

namespace Snap.Hutao.Server.Service.Redeem;

// Scoped
public sealed class RedeemService
{
    private readonly GachaLogExpireService gachaLogExpireService;
    private readonly CdnExpireService cdnExpireService;
    private readonly AppDbContext appDbContext;

    public RedeemService(IServiceProvider serviceProvider)
    {
        gachaLogExpireService = serviceProvider.GetRequiredService<GachaLogExpireService>();
        cdnExpireService = serviceProvider.GetRequiredService<CdnExpireService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task<RedeemUseResponse> UseRedeemCodeAsync(RedeemUseRequest request)
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

        if (code.Type is 0U)
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
                await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
                return new(RedeemStatus.Ok, code.ServiceType, result.ExpiredAt);
            }
        }

        if ((code.Type & 0b001U) > 0U)
        {
            if (code.ExpireTime < DateTimeOffset.UtcNow)
            {
                return new(RedeemStatus.Expired);
            }

            if (await this.ExtendTermForUserNameByCodeAsync(request.Username, code).ConfigureAwait(false) is { Kind: TermExtendResultKind.Ok } result)
            {
                code.UseCount++;
                await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
                return new(RedeemStatus.Ok, code.ServiceType, result.ExpiredAt);
            }
        }

        if ((code.Type & 0b010U) > 0U)
        {
            if (code.TimesRemain is 0U)
            {
                return new(RedeemStatus.NotEnough);
            }

            if (await this.ExtendTermForUserNameByCodeAsync(request.Username, code).ConfigureAwait(false) is { Kind: TermExtendResultKind.Ok } result)
            {
                code.TimesRemain--;
                await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
                return new(RedeemStatus.Ok, code.ServiceType, result.ExpiredAt);
            }
        }

        return new(RedeemStatus.DbError);
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

            if ((req.Type & 0b001U) > 0U)
            {
                code.ExpireTime = req.ExpireTime;
            }

            if ((req.Type & 0b010U) > 0U)
            {
                code.TimesRemain = req.Times;
            }

            codes.Add(code.Code);
            await appDbContext.RedeemCodes.AddAndSaveAsync(code).ConfigureAwait(false);
        }

        return new(codes);
    }

    private ValueTask<TermExtendResult> ExtendTermForUserNameByCodeAsync(string username, RedeemCode code)
    {
        IExpireService expireService = code.ServiceType switch
        {
            1 => gachaLogExpireService,
            2 => cdnExpireService,
            _ => throw new NotSupportedException(),
        };

        return expireService.ExtendTermForUserNameAsync(username, code.Value);
    }
}