// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Controller.Filter;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 祈愿记录控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class GachaLogController : ControllerBase
{
    /// <summary>
    /// 获取祈愿记录
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>祈愿记录</returns>
    [HttpGet("Retrive")]
    public async Task<IActionResult> RetriveAsync([FromQuery(Name = "uid")]string uid)
    {
        await Task.Yield();
        return Model.Response.Response.Success("testing");
    }

    /// <summary>
    /// 上传祈愿记录
    /// </summary>
    /// <returns>上传成功</returns>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync()
    {
        await Task.Yield();
        return Model.Response.Response.Success("testing");
    }
}