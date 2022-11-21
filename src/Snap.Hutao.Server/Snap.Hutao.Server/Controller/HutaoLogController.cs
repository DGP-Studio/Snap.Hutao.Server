// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;

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
    /// <param name="uploadLog">日志</param>
    /// <returns>任务</returns>
    [HttpPost("Upload")]
    public IActionResult Upload([FromBody] HutaoUploadLog uploadLog)
    {
        if (uploadLog.Id == null)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "数据不完整");
        }

        if (uploadLog.Time == 0)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "数据不完整");
        }

        string info = uploadLog.Info;
        HutaoLog? log = appDbContext.HutaoLogs.SingleOrDefault(log => log.Info == info);

        if (log != null)
        {
            if (!log.Resolved)
            {
                log.Count += 1;
                log.Time = uploadLog.Time;
                appDbContext.SaveChanges();
            }
        }
        else
        {
            appDbContext.HutaoLogs.Add(new HutaoLog
            {
                Id = uploadLog.Id,
                Time = uploadLog.Time,
                Info = info,
                Count = 1,
            });
            appDbContext.SaveChanges();
        }

        return Model.Response.Response.Success("日志上传成功");
    }

    /// <summary>
    /// 获取未完成的列表
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns>未完成的列表</returns>
    [HttpGet("Unresolved")]
    public IActionResult Unresolved([FromQuery]long time)
    {
        List<HutaoLog> result = appDbContext.HutaoLogs
            .AsNoTracking()
            .OrderBy(log => log.Time)
            .Where(log => log.Time > time)
            .Where(log => log.Resolved == false)
            .Take(10)
            .ToList();

        return Response<List<HutaoLog>>.Success("获取记录成功", result);
    }

    /// <summary>
    /// 标记为已经解决
    /// </summary>
    /// <param name="pid">主键Id</param>
    /// <returns>结果</returns>
    [HttpGet("Resolve")]
    public IActionResult Resolve([FromQuery] long pid)
    {
        HutaoLog? log = appDbContext.HutaoLogs.SingleOrDefault(l => l.PrimaryId == pid);

        if (log != null)
        {
            log.Resolved = true;
            appDbContext.SaveChanges();
            return Model.Response.Response.Success("成功标记");
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.NoMatchedLogId, "无对应的日志Id");
        }
    }
}