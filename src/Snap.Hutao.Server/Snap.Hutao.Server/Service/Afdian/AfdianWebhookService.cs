// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using MailKit.Net.Smtp;
using MimeKit;
using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Expire;
using Snap.Hutao.Server.Service.Redeem;

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
    private readonly RedeemService redeemService;

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
        redeemService = serviceProvider.GetRequiredService<RedeemService>();
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

        if (order is not { SkuDetail: [{ } skuDetail, ..] })
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
            switch (result.Kind)
            {
                case TermExtendResultKind.Ok:
                    await mailService.SendPurchaseGachaLogStorageServiceAsync(info.UserName, result.ExpiredAt.ToOffset(new(8, 0, 0)).ToString("yyy/MM/dd HH:mm:sszzz"), info.OrderNumber).ConfigureAwait(false);
                    break;
                case TermExtendResultKind.NoSuchUser:
                    string redeemCode = await redeemService.GenerateRedeemCodeForPurchaseGachaLogServiceAsync(info).ConfigureAwait(false);
                    try
                    {
                        await mailService.SendPurchaseGachaLogStorageServiceNoSuchUserAsync(info.UserName, redeemCode, info.OrderNumber).ConfigureAwait(false);
                        info.Status = AfdianOrderStatus.GachaLogTermExtendNoSuchUser;
                    }
                    catch (SmtpCommandException)
                    {
                        info.Status = AfdianOrderStatus.InvalidUserName;
                    }
                    catch (ParseException)
                    {
                        info.Status = AfdianOrderStatus.InvalidUserName;
                    }

                    break;
                case TermExtendResultKind.DbError:
                    info.Status = AfdianOrderStatus.GachaLogTermExtendDbError;
                    break;
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
            switch (result.Kind)
            {
                case TermExtendResultKind.Ok:
                    await mailService.SendPurchaseCdnServiceAsync(info.UserName, result.ExpiredAt.ToOffset(new(8, 0, 0)).ToString("yyy/MM/dd HH:mm:sszzz"), info.OrderNumber).ConfigureAwait(false);
                    break;
                case TermExtendResultKind.NoSuchUser:
                    string redeemCode = await redeemService.GenerateRedeemCodeForPurchaseCdnServiceAsync(info).ConfigureAwait(false);
                    try
                    {
                        await mailService.SendPurchaseCdnServiceNoSuchUserAsync(info.UserName, redeemCode, info.OrderNumber).ConfigureAwait(false);
                        info.Status = AfdianOrderStatus.CdnTermExtendNoSuchUser;
                    }
                    catch (SmtpCommandException)
                    {
                        info.Status = AfdianOrderStatus.InvalidUserName;
                    }
                    catch (ParseException)
                    {
                        info.Status = AfdianOrderStatus.InvalidUserName;
                    }

                    break;
                case TermExtendResultKind.DbError:
                    info.Status = AfdianOrderStatus.CdnTermExtendDbError;
                    break;
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

        HttpResponseMessage? response = default;

        int requestCount = 0;
        while (++requestCount <= 10)
        {
            response = await httpClient.PostAsJsonAsync("https://afdian.com/api/open/query-order", query).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                break;
            }

            response.Dispose();
            await Task.Delay(3000).ConfigureAwait(false);
        }

        AfdianResponse<ListWrapper<Order>>? resp;
        using (response)
        {
            if (response is not { IsSuccessStatusCode: true })
            {
                info.Status = AfdianOrderStatus.ValidationRequestFailed;
                return false;
            }

            resp = await response.Content.ReadFromJsonAsync<AfdianResponse<ListWrapper<Order>>>().ConfigureAwait(false);
            if (resp is not { ErrorCode: 200 })
            {
                info.Status = AfdianOrderStatus.ValidationResponseInvalid;
                return false;
            }
        }

        if (resp is not { Data.List: [{ } order, ..] })
        {
            info.Status = AfdianOrderStatus.ValidationResponseNoOrder;
            return false;
        }

        if (order is not { SkuDetail: [{ } skuDetail, ..] })
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