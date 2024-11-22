// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Distribution;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(GroupName = "Distribution")]
public class DistributionController : ControllerBase
{
    private readonly DistributionService distributionService;

    public DistributionController(DistributionService distributionService)
    {
        this.distributionService = distributionService;
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("GetAcceleratedMirror")]
    public async Task<IActionResult> GetAcceleratedMirror([FromQuery(Name = "Filename")] string filename)
    {
        HutaoPackageMirror? mirror = await distributionService.GetAcceleratedMirrorAsync(filename);
        return Response<HutaoPackageMirror?>.Success("获取加速镜像成功", mirror);
    }
}