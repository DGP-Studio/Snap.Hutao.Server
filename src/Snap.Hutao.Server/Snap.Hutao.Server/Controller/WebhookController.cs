// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Service;
using System.Text.Json;

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
    private readonly UserManager<HutaoUser> userManager;
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
        userManager = serviceProvider.GetRequiredService<UserManager<HutaoUser>>();
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
    public async Task<IActionResult> IncomingAfdianAsync([FromBody] JsonElement request /*[FromBody] AfdianResponse<OrderWrapper> request*/)
    {
        logger.LogInformation("UserName:{name}", request);
        return new JsonResult(new AfdianResponse() { ErrorCode = 200, ErrorMessage = string.Empty });

        //string userName = request.Data.Order.Remark;
        //logger.LogInformation("UserName:{name}", request.Data.Order.Remark);

        //string tradeNumber = request.Data.Order.OutTradeNo;

        //if (request.Data.Order.SkuDetail.FirstOrDefault() is SkuDetail skuDetail)
        //{
        //    // GachaLog Upload
        //    if (skuDetail.SkuId == "80d6dcb8cf9011ed9c3652540025c377")
        //    {
        //        string skuId = skuDetail.SkuId;
        //        int count = skuDetail.Count;

        //        if (await ValidateTradeAsync(tradeNumber, skuId, count).ConfigureAwait(false))
        //        {
        //            HutaoUser? user = await userManager.FindByNameAsync(userName).ConfigureAwait(false);

        //            if (user != null)
        //            {
        //                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //                if (user.GachaLogExpireAt < now)
        //                {
        //                    user.GachaLogExpireAt = now;
        //                }

        //                user.GachaLogExpireAt += (long)TimeSpan.FromDays(30 * count).TotalSeconds;

        //                IdentityResult result = await userManager.UpdateAsync(user).ConfigureAwait(false);

        //                if (result.Succeeded)
        //                {
        //                    string expireAt = DateTimeOffset.FromUnixTimeSeconds(user.GachaLogExpireAt).ToString("yyy MM dd HH:mm:ss");
        //                    await mailService.SendPurchaseGachaLogStorageServiceAsync(userName, expireAt, tradeNumber).ConfigureAwait(false);
        //                }
        //            }
        //        }
        //    }
        //}

        //return new JsonResult(new AfdianResponse() { ErrorCode = 200, ErrorMessage = string.Empty });
    }

    private async Task<bool> ValidateTradeAsync(string tradeNumber, string skuId, int count)
    {
        QueryOrder query = QueryOrder.Create(UserId, tradeNumber, afdianToken);
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
                }
            }
        }

        return false;
    }
}