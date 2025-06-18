// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Distribution;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Distribution")]
public class DistributionController : ControllerBase
{
    private readonly DistributionService distributionService;

    public DistributionController(DistributionService distributionService)
    {
        this.distributionService = distributionService;
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateCdnPermission))]
    [HttpGet("GetDownloadToken")]
    public async Task<IActionResult> GetDownloadToken()
    {
        string? token = await distributionService.GetDownloadTokenAsync();
        if (token is null)
        {
            return Model.Response.Response.Fail(ReturnCode.CdnDispatcherException, "获取下载令牌失败");
        }

        return Response<string>.Success("获取下载令牌成功", token);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateCdnPermission))]
    [HttpGet("GetAcceleratedMirror")]
    public async Task<IActionResult> GetAcceleratedMirror([FromQuery(Name = "Filename")] string filename)
    {
        HutaoPackageMirror? mirror = await distributionService.GetAcceleratedMirrorAsync(filename);
        if (mirror is null)
        {
            return Model.Response.Response.Fail(ReturnCode.CdnDispatcherException, "获取加速镜像失败");
        }

        return Response<HutaoPackageMirror?>.Success("获取加速镜像成功", mirror);
    }
}