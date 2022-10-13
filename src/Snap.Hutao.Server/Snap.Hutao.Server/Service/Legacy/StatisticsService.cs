﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Service.Legacy;

/// <summary>
/// 统计服务
/// </summary>
public class StatisticsService
{
    /// <summary>
    /// 统计服务正在工作
    /// </summary>
    public const string Working = "StatisticsService.Working";

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的统计服务
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    public StatisticsService(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 异步计算结果
    /// </summary>
    /// <returns>任务</returns>
    public async Task RunAsync()
    {
        StatisticsTracker tracker = new();
        using (memoryCache.Flag(Working))
        {
            using (await appDbContext.OperationLock.EnterAsync().ConfigureAwait(false))
            {
                await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
                tracker.CompleteTracking(appDbContext, memoryCache);
            }
        }
    }

    private void RunCore(StatisticsTracker tracker)
    {
        const int partion = 100;

        while (true)
        {
            // keyset 分页获取，降低内存压力
            List<EntityRecord> part = appDbContext.Records
                .OrderBy(r => r.PrimaryId)
                .Where(r => r.PrimaryId > tracker.LastId)

                // https://learn.microsoft.com/en-us/ef/core/miscellaneous/nullable-reference-types#navigating-and-including-nullable-relationships
                .Include(r => r.SpiralAbyss!)
                .ThenInclude(s => s.Floors)
                .Include(r => r.Avatars)
                .Take(partion)
                .ToList();

            foreach (EntityRecord record in part)
            {
                tracker.Track(record);
            }

            if (part.Count < partion)
            {
                break;
            }
        }
    }
}