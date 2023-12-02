// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.GachaLog;

namespace Snap.Hutao.Server.Service.Afdian;

// Singleton
public sealed class AfdianWebhookService
{
    private readonly DiscordService discordService;
    private readonly IMemoryCache memoryCache;
    private readonly AfdianOptions afdianOptions;
    private readonly ExpireService expireService;
    private readonly MailService mailService;
    private readonly HttpClient httpClient;

    public AfdianWebhookService(IServiceProvider serviceProvider)
    {
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        afdianOptions = serviceProvider.GetRequiredService<AppOptions>().Afdian;
        expireService = serviceProvider.GetRequiredService<ExpireService>();
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
            if (!await ValidateOrderInformationAsync(info).ConfigureAwait(false))
            {
                // info's status will be set in ValidateOrderInformationAsync
                return info;
            }

            GachaLogTermExtendResult result = await expireService.ExtendGachaLogTermForAfdianOrderAsync(info).ConfigureAwait(false);
            if (result.Kind is GachaLogTermExtendResultKind.Ok)
            {
                await mailService.SendPurchaseGachaLogStorageServiceAsync(info.UserName, result.ExpiredAt.ToString("yyy/MM/dd HH:mm:ss"), info.OrderNumber).ConfigureAwait(false);
            }
        }
        else
        {
            info.Status = AfdianOrderStatus.SkuIdNotSupported;
        }

        return info;
    }

    private async ValueTask<bool> ValidateOrderInformationAsync(AfdianOrderInformation info)
    {
        QueryOrder query = QueryOrder.Create(afdianOptions.UserId, info.OrderNumber, afdianOptions.UserToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("https://afdian.net/api/open/query-order", query).ConfigureAwait(false);
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