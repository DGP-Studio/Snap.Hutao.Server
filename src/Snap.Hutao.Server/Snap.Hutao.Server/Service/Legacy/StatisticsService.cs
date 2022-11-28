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
    private const int Partion = 200;

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    // comple queries that used multiple times to increase performance
    private readonly Func<AppDbContext, long, IEnumerable<EntityRecord>> partionQuery = EF.CompileQuery((AppDbContext context, long lastId) =>
        context.Records.AsNoTracking().OrderBy(r => r.PrimaryId).Where(r => r.PrimaryId > lastId).Take(Partion));

    private readonly Func<AppDbContext, long, IEnumerable<EntityAvatar>> avatarQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.Avatars.AsNoTracking().Where(a => a.RecordId == recordId));

    private readonly Func<AppDbContext, long, EntitySpiralAbyss?> spiralAbyssQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.SpiralAbysses.AsNoTracking().SingleOrDefault(s => s.RecordId == recordId));

    private readonly Func<AppDbContext, long, IEnumerable<EntityFloor>> floorQuery = EF.CompileQuery((AppDbContext context, long spiralAbyssId) =>
        context.SpiralAbyssFloors.AsNoTracking().Where(f => f.SpiralAbyssId == spiralAbyssId));

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
        while (true)
        {
            List<EntityRecord> part = partionQuery(appDbContext, tracker.LastId).ToList();

            foreach (EntityRecord record in part)
            {
                record.Avatars = avatarQuery(appDbContext, record.PrimaryId).ToList();
                record.SpiralAbyss = spiralAbyssQuery(appDbContext, record.PrimaryId);

                if (record.SpiralAbyss != null)
                {
                    record.SpiralAbyss.Floors = floorQuery(appDbContext, record.SpiralAbyss.PrimaryId).ToList();
                }

                tracker.Track(record);
            }

            if (part.Count < Partion)
            {
                break;
            }
        }
    }
}