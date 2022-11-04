// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 胡桃日志控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class HutaoLogController : ControllerBase
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 构造一个新的胡桃日志控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    public HutaoLogController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <summary>
    /// 上传日志
    /// </summary>
    /// <param name="hutaoLog">日志</param>
    /// <returns>任务</returns>
    [HttpPost("Upload")]
    public IActionResult Upload([FromBody] HutaoLog hutaoLog)
    {
        if (hutaoLog.Id == null)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "数据不完整");
        }

        if (hutaoLog.Time == 0)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "数据不完整");
        }

        appDbContext.HutaoLogs.Add(hutaoLog);
        appDbContext.SaveChanges();

        return Model.Response.Response.Success("日志上传成功");
    }
}