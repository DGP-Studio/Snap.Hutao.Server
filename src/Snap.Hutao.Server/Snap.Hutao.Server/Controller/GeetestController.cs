// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Geetest;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class GeetestController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly AppOptions appOptions;

    public GeetestController(HttpClient httpClient, AppOptions appOptions)
    {
        this.httpClient = httpClient;
        this.appOptions = appOptions;
    }

    [HttpGet("Verify")]
    public async Task<IActionResult> VerifyAsync([FromQuery] string gt, [FromQuery] string challenge)
    {
        string url = string.Format(appOptions.GeetestImplementationFormat, gt, challenge);
        GeetestResponse? result = await httpClient.GetFromJsonAsync<GeetestResponse>(url).ConfigureAwait(false);
        return new JsonResult(result);
    }
}