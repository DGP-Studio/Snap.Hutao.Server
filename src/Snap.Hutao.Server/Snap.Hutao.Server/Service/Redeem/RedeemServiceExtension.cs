// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.Redeem;

public static class RedeemServiceExtension
{
    public static async ValueTask<string> GenerateRedeemCodeForPurchaseGachaLogServiceAsync(this RedeemService redeemService, AfdianOrderInformation info)
    {
        RedeemGenerateRequest req = new()
        {
            Count = 1U,
            Type = RedeemCodeType.TimeLimited | RedeemCodeType.TimesLimited,
            ServiceType = RedeemCodeTargetServiceType.GachaLog,
            Value = 30 * info.OrderCount,
            Description = "For unregistered user",
            ExpireTime = DateTimeOffset.UtcNow + TimeSpan.FromDays(3),
            Times = 1,
            Creator = "AfdianWebhookService",
        };

        RedeemGenerateResponse codes = await redeemService.GenerateRedeemCodesAsync(req).ConfigureAwait(false);
        return codes.Codes.Single();
    }

    public static async ValueTask<string> GenerateRedeemCodeForPurchaseCdnServiceAsync(this RedeemService redeemService, AfdianOrderInformation info)
    {
        RedeemGenerateRequest req = new()
        {
            Count = 1U,
            Type = RedeemCodeType.TimeLimited | RedeemCodeType.TimesLimited,
            ServiceType = RedeemCodeTargetServiceType.Cdn,
            Value = 30 * info.OrderCount,
            Description = "For unregistered user",
            ExpireTime = DateTimeOffset.UtcNow + TimeSpan.FromDays(3),
            Times = 1,
            Creator = "AfdianWebhookService",
        };

        RedeemGenerateResponse codes = await redeemService.GenerateRedeemCodesAsync(req).ConfigureAwait(false);
        return codes.Codes.Single();
    }
}