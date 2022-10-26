﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Controller.Helper;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Legacy;
using System.Collections.Concurrent;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 记录控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class RecordController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;
    private readonly ConcurrentDictionary<string, UploadToken> uidUploading = new();
    private bool isRanking;

    /// <summary>
    /// 构造一个新的记录控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    public RecordController(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 上传记录
    /// </summary>
    /// <param name="record">记录</param>
    /// <returns>上传结果</returns>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] SimpleRecord record)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        string recordUid = record.Uid;
        bool isBanned = appDbContext.BannedList.Any(banned => banned.Uid == recordUid);

        if (isBanned)
        {
            return Model.Response.Response.Fail(ReturnCode.BannedUid, "Uid 已被数据库封禁");
        }

        if (!record.Validate())
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "无效的提交数据");
        }

        if (uidUploading.TryGetValue(record.Uid, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理");
        }

        if (uidUploading.TryAdd(record.Uid, new()))
        {
            await RecordHelper.SaveRecordAsync(appDbContext, record).ConfigureAwait(false);

            if (uidUploading.TryRemove(record.Uid, out _))
            {
                return Model.Response.Response.Success("数据提交成功");
            }
            else
            {
                return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常");
            }
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.InternalStateException, "提交状态异常");
        }
    }

    /// <summary>
    /// 检查uid对应的记录是否存在
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>是否存在记录</returns>
    [HttpGet("Check")]
    public async Task<IActionResult> CheckAsync([FromQuery(Name = "uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid");
        }

        using (await appDbContext.OperationLock.EnterAsync().ConfigureAwait(false))
        {
            EntityRecord? record = appDbContext.Records.SingleOrDefault(r => r.Uid == uid);

            if (record != null)
            {
                long recordId = record.PrimaryId;
                EntitySpiralAbyss? spiralAbyss = appDbContext.SpiralAbysses.SingleOrDefault(r => r.RecordId == recordId);

                if (spiralAbyss != null)
                {
                    return Response<bool>.Success("查询成功", true);
                }
            }

            return Response<bool>.Success("查询成功", false);
        }
    }

    /// <summary>
    /// 获取排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    [HttpGet("Rank")]
    public async Task<IActionResult> RankAsync([FromQuery(Name = "uid")] string uid)
    {
        if (memoryCache.TryGetValue(StatisticsService.Working, out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据");
        }

        if (!int.TryParse(uid, out _) || uid.Length != 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid");
        }

        if (isRanking)
        {
            return Model.Response.Response.Fail(ReturnCode.RequestTooFrequent, $"上个排行请求仍在处理中");
        }
        else
        {
            isRanking = true;
            Rank rank;
            using (await appDbContext.OperationLock.EnterAsync().ConfigureAwait(false))
            {
                int scheduleId = StatisticsHelper.GetScheduleId();

                RankValue? damageRank = RankHelper.GetDamageRank(appDbContext, memoryCache, scheduleId, uid);
                RankValue? takeDamageRank = RankHelper.GetTakeDamageRank(appDbContext, memoryCache, scheduleId, uid);
                rank = new(damageRank, takeDamageRank);
            }

            isRanking = false;
            return Response<Rank>.Success("获取排行数据成功", rank);
        }
    }

    /// <summary>
    /// 并行上传占位
    /// </summary>
    private struct UploadToken
    {
    }
}