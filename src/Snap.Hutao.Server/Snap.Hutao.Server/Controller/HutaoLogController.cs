// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 胡桃日志控制器
/// </summary>
[ApiController]
[Route("[controller]")]
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
        string version = Request.Headers.UserAgent.ToString();
        HutaoLog? log = appDbContext.HutaoLogs.SingleOrDefault(log => log.Info == info);

        if (log != null)
        {
            log.Count += 1;
            log.Version = version;
        }
        else
        {
            log = new HutaoLog
            {
                Info = info,
                Count = 1,
                Version = version,
            };
            appDbContext.HutaoLogs.Add(log);
        }

        appDbContext.SaveChanges();

        HutaoLogSingleItem singleItem = new()
        {
            LogId = log.PrimaryId,
            DeviceId = uploadLog.Id,
            Time = uploadLog.Time,
        };
        appDbContext.HutaoLogSingleItems.AddAndSave(singleItem);

        return Model.Response.Response.Success("日志上传成功");
    }

    /// <summary>
    /// 按设备Id给出最近3次的崩溃信息
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <returns>最近3次的崩溃信息</returns>
    [HttpGet("ByDeviceId")]
    public IActionResult ByDeviceId([FromQuery(Name = "id")] string deviceId)
    {
        List<HutaoLogSingleItem> items = appDbContext.HutaoLogSingleItems
            .AsNoTracking()
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.Time)
            .Take(3)
            .ToList();

        List<HutaoLog> logs = items.SelectList(item => appDbContext.HutaoLogs.AsNoTracking().Single(x => x.PrimaryId == item.LogId));
        return Response<List<HutaoLog>>.Success("获取记录成功", logs);
    }
}