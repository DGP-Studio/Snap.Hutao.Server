// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Expire;

namespace Snap.Hutao.Server.Service.Afdian;

// Singleton
public sealed class AfdianWebhookService
{
    private readonly DiscordService discordService;
    private readonly IMemoryCache memoryCache;
    private readonly AfdianOptions afdianOptions;
    private readonly Afdian2Options afdian2Options;
    private readonly GachaLogExpireService gachaLogExpireService;
    private readonly CdnExpireService cdnExpireService;
    private readonly MailService mailService;
    private readonly HttpClient httpClient;

    public AfdianWebhookService(IServiceProvider serviceProvider)
    {
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        afdianOptions = serviceProvider.GetRequiredService<AppOptions>().Afdian;
        afdian2Options = serviceProvider.GetRequiredService<AppOptions>().Afdian2;
        gachaLogExpireService = serviceProvider.GetRequiredService<GachaLogExpireService>();
        cdnExpireService = serviceProvider.GetRequiredService<CdnExpireService>();
        mailService = serviceProvider.GetRequiredService<MailService>();
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
    }

    public async ValueTask ProcessIncomingOrderAsync(Order order)
    {
        string orderNumber = order.OutTradeNo;
        if (memoryCache.TryGetValue(orderNumber, out _))
        {
            return;
        }

        // prevent multiple requests in 10 minutes
        memoryCache.Set(orderNumber, default(RequestToken), TimeSpan.FromMinutes(10));

        AfdianOrderInformation info = await ProcessIncomingOrderCoreAsync(order).ConfigureAwait(false);
        await discordService.ReportAfdianOrderAsync(info).ConfigureAwait(false);
    }

    private async ValueTask<AfdianOrderInformation> ProcessIncomingOrderCoreAsync(Order order)
    {
        AfdianOrderInformation info = new()
        {
            OrderNumber = order.OutTradeNo,
            UserName = order.Remark, // We use remark to get the userName(email) of the user
        };

        if (order is not { SkuDetail: [SkuDetail skuDetail, ..] })
        {
            info.Status = AfdianOrderStatus.NoSkuDetails;
            return info;
        }

        info.SkuId = skuDetail.SkuId;
        info.OrderCount = skuDetail.Count;

        // GachaLog Upload
        if (skuDetail.SkuId == afdianOptions.SkuGachaLogUploadService)
        {
            if (!await ValidateOrderInformationAsync(info, afdianOptions).ConfigureAwait(false))
            {
                // info's status will be set in ValidateOrderInformationAsync
                return info;
            }

            TermExtendResult result = await gachaLogExpireService.ExtendGachaLogTermForAfdianOrderAsync(info).ConfigureAwait(false);
            if (result.Kind is TermExtendResultKind.Ok)
            {
                await mailService.SendPurchaseGachaLogStorageServiceAsync(info.UserName, result.ExpiredAt.ToOffset(new(8, 0, 0)).ToString("yyy/MM/dd HH:mm:sszzz"), info.OrderNumber).ConfigureAwait(false);
            }
            else
            {
                info.Status = result.Kind switch
                {
                    TermExtendResultKind.NoSuchUser => AfdianOrderStatus.GachaLogTermExtendNoSuchUser,
                    TermExtendResultKind.DbError => AfdianOrderStatus.GachaLogTermExtendDbError,
                    _ => info.Status,
                };
            }
        }
        else if (skuDetail.SkuId == afdian2Options.SkuCdnService)
        {
            if (!await ValidateOrderInformationAsync(info, afdian2Options).ConfigureAwait(false))
            {
                // info's status will be set in ValidateOrderInformationAsync
                return info;
            }

            TermExtendResult result = await cdnExpireService.ExtendCdnTermForAfdianOrderAsync(info).ConfigureAwait(false);
            if (result.Kind is TermExtendResultKind.Ok)
            {
                await mailService.SendPurchaseCdnServiceAsync(info.UserName, result.ExpiredAt.ToOffset(new(8, 0, 0)).ToString("yyy/MM/dd HH:mm:sszzz"), info.OrderNumber).ConfigureAwait(false);
            }
            else
            {
                info.Status = result.Kind switch
                {
                    TermExtendResultKind.NoSuchUser => AfdianOrderStatus.CdnTermExtendNoSuchUser,
                    TermExtendResultKind.DbError => AfdianOrderStatus.CdnTermExtendDbError,
                    _ => info.Status,
                };
            }
        }
        else
        {
            info.Status = AfdianOrderStatus.SkuIdNotSupported;
        }

        return info;
    }

    private async ValueTask<bool> ValidateOrderInformationAsync(AfdianOrderInformation info, AfdianAuthorizationOptions afdianOptions)
    {
        QueryOrder query = QueryOrder.Create(afdianOptions.UserId, info.OrderNumber, afdianOptions.UserToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("https://afdian.com/api/open/query-order", query).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            info.Status = AfdianOrderStatus.ValidationRequestFailed;
            return false;
        }

        AfdianResponse<ListWrapper<Order>>? resp = await response.Content.ReadFromJsonAsync<AfdianResponse<ListWrapper<Order>>>().ConfigureAwait(false);
        if (resp is not { ErrorCode: 200 })
        {
            info.Status = AfdianOrderStatus.ValidationResponseInvalid;
            return false;
        }

        if (resp is not { Data.List: [Order order, ..] })
        {
            info.Status = AfdianOrderStatus.ValidationResponseNoOrder;
            return false;
        }

        if (order is not { SkuDetail: [SkuDetail skuDetail, ..] })
        {
            info.Status = AfdianOrderStatus.ValidationResponseNoSkuDetail;
            return false;
        }

        if (order.OutTradeNo != info.OrderNumber || skuDetail.SkuId != info.SkuId || skuDetail.Count != info.OrderCount)
        {
            info.Status = AfdianOrderStatus.ValidationResponseSkuDetailNotMatch;
            return false;
        }

        return true;
    }

    private struct RequestToken;
}