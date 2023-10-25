// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Controller.Helper;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.Legacy.PizzaHelper;
using Snap.Hutao.Server.Service.Ranking;
using System.Collections.Concurrent;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 记录控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(GroupName = "SpiralAbyss")]
public class RecordController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, UploadToken> UidUploading = new();
    private readonly AppDbContext appDbContext;
    private readonly IRankService rankService;
    private readonly ExpireService expireService;
    private readonly PizzaHelperRecordService pizzaHelperRecordService;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的记录控制器
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public RecordController(IServiceProvider serviceProvider)
    {
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        rankService = serviceProvider.GetRequiredService<IRankService>();
        expireService = serviceProvider.GetRequiredService<ExpireService>();
        pizzaHelperRecordService = serviceProvider.GetRequiredService<PizzaHelperRecordService>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    }

    /// <summary>
    /// 上传记录
    /// </summary>
    /// <param name="record">记录</param>
    /// <param name="returningRank">返回排行结果</param>
    /// <returns>上传结果</returns>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] SimpleRecord record, [FromQuery] bool returningRank = false)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", "ServerRecordComputingStatistics");
        }

        if (appDbContext.BannedList.Any(banned => banned.Uid == record.Uid))
        {
            return Model.Response.Response.Fail(ReturnCode.BannedUid, "Uid 已被数据库封禁", "ServerRecordBannedUid");
        }

        if (record.SpiralAbyss.ScheduleId != StatisticsHelper.GetScheduleId())
        {
            return Model.Response.Response.Fail(ReturnCode.NotCurrentSchedule, "非当前深渊数据", "ServerRecordNotCurrentSchedule");
        }

        if (!record.Validate())
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "无效的提交数据", "ServerRecordInvalidData");
        }

        if (UidUploading.TryGetValue(record.Uid, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理", "ServerRecordPreviousRequestNotCompleted");
        }

        if (!UidUploading.TryAdd(record.Uid, new()))
        {
            return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常", "ServerRecordInternalException");
        }

        RecordUploadResult result = await RecordHelper.SaveRecordAsync(appDbContext, rankService, expireService, record).ConfigureAwait(false);
        await pizzaHelperRecordService.TryPostRecordAsync(record).ConfigureAwait(false);

        if (!UidUploading.TryRemove(record.Uid, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常", "ServerRecordInternalException");
        }

        if (returningRank)
        {
            Rank rank = await rankService.RetriveRankAsync(record.Uid).ConfigureAwait(false);
            return Response<Rank>.Success("获取排行数据成功", rank);
        }
        else
        {
            return result switch
            {
                RecordUploadResult.GachaLogExtented => Model.Response.Response.Success("数据提交成功，获赠 3 天祈愿记录上传服务时长", "ServerRecordUploadSuccessAndGachaLogServiceTimeExtended"),
                RecordUploadResult.NotSnapHutao => Model.Response.Response.Success("数据提交成功，但不是由胡桃客户端发起，无法获赠祈愿记录上传服务时长"),
                RecordUploadResult.NotFirstAttempt => Model.Response.Response.Success("数据提交成功，但不是本期首次提交，无法获赠祈愿记录上传服务时长", "ServerRecordUploadSuccessButNotFirstTimeAtCurrentSchedule"),
                RecordUploadResult.NoUserNamePresented => Model.Response.Response.Success("数据提交成功，但未绑定胡桃账号，无法获赠祈愿记录上传服务时长", "ServerRecordUploadSuccessButNoPassport"),
                _ => Model.Response.Response.Success("数据提交成功"),
            };
        }
    }

    /// <summary>
    /// 检查uid对应的记录是否存在
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>是否存在记录</returns>
    [HttpGet("Check")]
    public async Task<IActionResult> CheckAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", "ServerRecordComputingStatistics2");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid", "ServerRecordInvalidUid");
        }

        EntityRecord? record = appDbContext.Records.SingleOrDefault(r => r.Uid == uid);

        if (record != null)
        {
            long recordId = record.PrimaryId;
            EntitySpiralAbyss? spiralAbyss = await appDbContext.SpiralAbysses.SingleOrDefaultAsync(r => r.RecordId == recordId).ConfigureAwait(false);

            if (spiralAbyss != null)
            {
                return Response<bool>.Success("查询成功", true);
            }
        }

        return Response<bool>.Success("查询成功", false);
    }

    /// <summary>
    /// 获取排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    [HttpGet("Rank")]
    public async Task<IActionResult> RankAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", "ServerRecordComputingStatistics2");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid", "ServerRecordInvalidUid");
        }

        Rank rank = await rankService.RetriveRankAsync(uid).ConfigureAwait(false);
        return Response<Rank>.Success("获取排行数据成功", rank);
    }

    /// <summary>
    /// 并行上传占位
    /// </summary>
    private struct UploadToken
    {
    }
}