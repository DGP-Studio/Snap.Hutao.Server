// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 记录控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class RecordController : ControllerBase
{
    /// <summary>
    /// 上传记录
    /// </summary>
    /// <param name="record">记录</param>
    /// <returns>上传结果</returns>
    [HttpPost("[Action]")]
    public async Task<IActionResult> Upload([FromBody] SimpleRecord record)
    {
        return null!;
    }

    /// <summary>
    /// 检查uid对应的记录是否存在
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>是否存在记录</returns>
    [HttpGet("[Action]")]
    public async Task<IActionResult> Check([FromQuery(Name = "uid")] string uid)
    {
        return null!;
    }

    /// <summary>
    /// 获取排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    [HttpGet("[Action]")]
    public async Task<IActionResult> Rank([FromQuery(Name = "uid")] string uid)
    {
        return null!;
    }
}