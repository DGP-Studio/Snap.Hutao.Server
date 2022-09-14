// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 祈愿记录控制器
/// </summary>
[Route("[controller]")]
[ApiController]
public class GachaLogController : ControllerBase
{
    /// <summary>
    /// 获取祈愿记录
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>祈愿记录</returns>
    [HttpGet("[Action]")]
    public async Task<IActionResult> Retrive([FromQuery(Name = "uid")]string uid)
    {
        return null!;
    }

    /// <summary>
    /// 上传祈愿记录
    /// </summary>
    /// <returns>上传成功</returns>
    [HttpPost("[Action]")]
    public async Task<IActionResult> Upload()
    {
        return null!;
    }
}