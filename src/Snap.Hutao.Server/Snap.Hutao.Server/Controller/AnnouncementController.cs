// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 公告控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class AnnouncementController : ControllerBase
{
    /// <summary>
    /// 获取公告
    /// </summary>
    /// <returns>公告信息</returns>
    [HttpGet("List")]
    public IActionResult List()
    {
        return Model.Response.Response.Success("testing");
    }
}