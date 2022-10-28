// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Core;
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
                ValueStopwatch stopwatch = ValueStopwatch.StartNew();
                await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
                tracker.CompleteTracking(appDbContext, memoryCache, stopwatch);
            }
        }
    }

    private void RunCore(StatisticsTracker tracker)
    {
        const int partion = 200;

        while (true)
        {
            List<EntityRecord> part = appDbContext.Records
                .AsNoTracking()
                .OrderBy(r => r.PrimaryId)
                .Where(r => r.PrimaryId > tracker.LastId)
                .Take(partion)
                .ToList();

            foreach (EntityRecord record in part)
            {
                record.Avatars = appDbContext.Avatars
                    .AsNoTracking()
                    .Where(a => a.RecordId == record.PrimaryId)
                    .ToList();

                record.SpiralAbyss = appDbContext.SpiralAbysses
                    .AsNoTracking()
                    .SingleOrDefault(s => s.RecordId == record.PrimaryId);

                if (record.SpiralAbyss != null)
                {
                    record.SpiralAbyss.Floors = appDbContext.SpiralAbyssFloors
                        .AsNoTracking()
                        .Where(f => f.SpiralAbyssId == record.SpiralAbyss.PrimaryId)
                        .ToList();
                }

                tracker.Track(record);
            }

            if (part.Count < partion)
            {
                break;
            }
        }
    }
}