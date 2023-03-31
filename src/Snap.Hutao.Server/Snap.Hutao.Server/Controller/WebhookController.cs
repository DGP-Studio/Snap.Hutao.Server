// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Model.Afdian;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// Webhook 控制器
/// </summary>
[Route("[controller]")]
[ApiController]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> logger;

    /// <summary>
    /// 构造一个新的Webhook 控制器
    /// </summary>
    /// <param name="logger">日志器</param>
    public WebhookController(ILogger<WebhookController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// 爱发电
    /// </summary>
    /// <param name="request">请求</param>
    /// <returns>结果</returns>
    [HttpPost("Incoming/Afdian")]
    public async Task<IActionResult> IncomingAfdianAsync([FromBody] AfdianRequest<OrderWrapper> request)
    {
        logger.LogInformation("UserName:{name}", request.Data.Order.Remark);

        using (StreamReader reader = new(HttpContext.Request.Body))
        {
            string body = await reader.ReadToEndAsync().ConfigureAwait(false);
            logger.LogInformation("Afdian Webhook:\n{body}", body);
        }

        return new JsonResult(new AfdianCallback() { ErrorCode = 200, ErrorMessage = string.Empty });
    }

    /// <summary>
    /// 爱发电
    /// </summary>
    /// <param name="request">请求</param>
    /// <returns>结果</returns>
    [HttpPost("Incoming/Afdian2")]
    public async Task<IActionResult> IncomingAfdian2Async([FromBody] string request)
    {
        logger.LogInformation("Afdian Webhook:\n{request}", request);
        return new JsonResult(new AfdianCallback() { ErrorCode = 200, ErrorMessage = string.Empty });
    }
}