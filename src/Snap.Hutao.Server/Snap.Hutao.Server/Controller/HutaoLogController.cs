// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Entity.Telemetry;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Telemetry;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class HutaoLogController : ControllerBase
{
    private readonly TelemetryService telemetryService;

    public HutaoLogController(TelemetryService telemetryService)
    {
        this.telemetryService = telemetryService;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> ProcessUploadLog([FromBody] HutaoUploadLog uploadLog)
    {
        return await telemetryService.ProcessCrashLogAsync(uploadLog, Request.Headers.UserAgent!) switch
        {
            CrashLogProcessResult.Ok => Model.Response.Response.Success("日志上传成功"),
            _ => Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "数据不完整"),
        };
    }

    [HttpGet("ByDeviceId")]
    public async Task<IActionResult> LatestByDeviceId([FromQuery(Name = "id")] string deviceId)
    {
        return Response<List<HutaoLog>>.Success("获取记录成功", await telemetryService.GetLogsByDeviceId(deviceId).ConfigureAwait(false));
    }
}