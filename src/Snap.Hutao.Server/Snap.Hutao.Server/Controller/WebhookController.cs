// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class WebhookController : ControllerBase
{
    private readonly AfdianWebhookService afdianWebhookService;

    public WebhookController(AfdianWebhookService afdianWebhookService)
    {
        this.afdianWebhookService = afdianWebhookService;
    }

    [HttpGet("Incoming/Afdian")]
    [HttpPost("Incoming/Afdian")]
    public async Task<IActionResult> IncomingAfdianAsync([FromBody] AfdianResponse<OrderWrapper> request)
    {
        await afdianWebhookService.ProcessIncomingOrderAsync(request.Data.Order).ConfigureAwait(false);
        return new JsonResult(new AfdianResponse() { ErrorCode = 200, ErrorMessage = string.Empty });
    }
}