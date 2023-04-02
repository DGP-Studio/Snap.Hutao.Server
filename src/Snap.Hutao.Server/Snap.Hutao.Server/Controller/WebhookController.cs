// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Service;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// Webhook 控制器
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
[Route("[controller]")]
[ApiController]
public class WebhookController : ControllerBase
{
    private const string UserId = "8f9ed3e87f4911ebacb652540025c377";
    private readonly ExpireService expireService;
    private readonly MailService mailService;
    private readonly HttpClient httpClient;
    private readonly ILogger<WebhookController> logger;
    private readonly string afdianToken;

    /// <summary>
    /// 构造一个新的Webhook 控制器
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public WebhookController(IServiceProvider serviceProvider)
    {
        expireService = serviceProvider.GetRequiredService<ExpireService>();
        mailService = serviceProvider.GetRequiredService<MailService>();
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
        logger = serviceProvider.GetRequiredService<ILogger<WebhookController>>();
        afdianToken = serviceProvider.GetRequiredService<IConfiguration>()["Afdian"]!;
    }

    /// <summary>
    /// 爱发电
    /// </summary>
    /// <param name="request">请求</param>
    /// <returns>结果</returns>
    [HttpGet("Incoming/Afdian")]
    [HttpPost("Incoming/Afdian")]
    public async Task<IActionResult> IncomingAfdianAsync([FromBody] AfdianResponse<OrderWrapper> request)
    {
        string userName = request.Data.Order.Remark;
        logger.LogInformation("UserName: {name}", request.Data.Order.Remark);
        string tradeNumber = request.Data.Order.OutTradeNo;

        if (request.Data.Order.SkuDetail.FirstOrDefault() is SkuDetail skuDetail)
        {
            // GachaLog Upload
            if (skuDetail.SkuId == "80d6dcb8cf9011ed9c3652540025c377")
            {
                string skuId = skuDetail.SkuId;
                int count = skuDetail.Count;

                if (await ValidateTradeAsync(tradeNumber, skuId, count).ConfigureAwait(false))
                {
                    await expireService.ExtendGachaLogTermAsync(userName, 30, async user =>
                    {
                        string expireAt = DateTimeOffset.FromUnixTimeSeconds(user.GachaLogExpireAt).ToString("yyy MM dd HH:mm:ss");
                        await mailService.SendPurchaseGachaLogStorageServiceAsync(userName, expireAt, tradeNumber).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
                else
                {
                    logger.LogInformation("Validation failed");
                }
            }
            else
            {
                logger.LogInformation("SKU Id:[{id}] not supported", skuDetail.SkuId);
            }
        }
        else
        {
            logger.LogInformation("No SKU info");
        }

        return new JsonResult(new AfdianResponse() { ErrorCode = 200, ErrorMessage = string.Empty });
    }

    private async Task<bool> ValidateTradeAsync(string tradeNumber, string skuId, int count)
    {
        QueryOrder query = QueryOrder.Create(UserId, tradeNumber, afdianToken);
        logger.LogInformation("Fetch data for trade: [{trade}]", tradeNumber);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("https://afdian.net/api/open/query-order", query).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        AfdianResponse<ListWrapper<Order>>? resp = await response.Content.ReadFromJsonAsync<AfdianResponse<ListWrapper<Order>>>().ConfigureAwait(false);

        if (resp != null && resp.ErrorCode == 200)
        {
            if (resp.Data.List.FirstOrDefault() is Order order)
            {
                if (order.SkuDetail.FirstOrDefault() is SkuDetail skuDetail)
                {
                    if (order.OutTradeNo == tradeNumber && skuDetail.SkuId == skuId && skuDetail.Count == count)
                    {
                        return true;
                    }
                    else
                    {
                        logger.LogInformation("Detail not matched");
                    }
                }
                else
                {
                    logger.LogInformation("No sku");
                }
            }
            else
            {
                logger.LogInformation("No matched order");
            }
        }
        else
        {
            logger.LogInformation("Bad Request, Not valid data");
        }

        return false;
    }
}