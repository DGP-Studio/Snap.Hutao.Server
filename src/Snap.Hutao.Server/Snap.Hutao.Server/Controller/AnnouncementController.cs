// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 公告控制器
/// </summary>
[Route("[controller]")]
[ApiController]
public class AnnouncementController : ControllerBase
{
    /// <summary>
    /// 获取公告
    /// </summary>
    /// <returns>公告信息</returns>
    [HttpGet("~/")]
    public IActionResult Index()
    {
        return null!;
    }
}