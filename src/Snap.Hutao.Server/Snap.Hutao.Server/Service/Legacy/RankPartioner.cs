// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;

namespace Snap.Hutao.Server.Service.Legacy;

/// <summary>
/// 排行分段器
/// </summary>
public class RankPartioner
{
    /// <summary>
    /// 异步构建排行分段
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <returns>排行分段</returns>
    public static Task MakeAsync(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        int scheduleId = StatisticsHelper.GetScheduleId();
        return Task.Run(() =>
        {
            MakeDamageRank(appDbContext, memoryCache, scheduleId);
            MakeTakeDamageRank(appDbContext, memoryCache, scheduleId);
        });
    }

    private static void MakeDamageRank(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId)
    {
        double totalCount = appDbContext.DamageRanks.Count();
        int lastValue = 0;
        int current = 0;

        List<RankPartion> rankPartions = new(200);

        if (totalCount > 100)
        {
            int partion = (int)Math.Ceiling(totalCount / 100);

            while (true)
            {
                List<EntityDamageRank> rank = appDbContext.DamageRanks
                    .OrderBy(r => r.Value)
                    .Where(r=>r.Value>lastValue)
                    .Take(partion)
                    .ToList();

                if (rank.Count > 0)
                {
                    current += rank.Count;
                    lastValue = rank.Last().Value;
                    rankPartions.Add(new() { Reference = current / totalCount, Value = lastValue });
                }

                if (rank.Count < partion)
                {
                    break;
                }
            }
        }

        StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.DamageRank, rankPartions);
    }

    private static void MakeTakeDamageRank(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId)
    {
        double totalCount = appDbContext.TakeDamageRanks.Count();
        int current = 0;

        List<RankPartion> rankPartions = new(200);

        if (totalCount > 100)
        {
            int partion = (int)Math.Ceiling(totalCount / 100);

            while (true)
            {
                List<EntityTakeDamageRank> rank = appDbContext.TakeDamageRanks
                    .OrderBy(r => r.Value)
                    .Take(partion)
                    .ToList();

                if (rank.Count > 0)
                {
                    current += rank.Count;
                    rankPartions.Add(new() { Reference = current / totalCount, Value = rank.Last().Value });
                }

                if (rank.Count < partion)
                {
                    break;
                }
            }
        }

        StatisticsHelper.SaveStatistics(appDbContext, memoryCache, scheduleId, LegacyStatistics.TakeDamageRank, rankPartions);
    }
}
