// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Controller.Helper;

/// <summary>
/// 排行帮助类
/// </summary>
public static class RankHelper
{
    /// <summary>
    /// 获取造成伤害榜
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="scheduleId">计划Id</param>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    public static RankValue? GetDamageRank(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId, string uid)
    {
        List<RankPartion>? damageRankPartion = StatisticsHelper.FromCacheOrDb<List<RankPartion>>(appDbContext, memoryCache, scheduleId, LegacyStatistics.DamageRank);
        if (damageRankPartion != null && damageRankPartion.Any())
        {
            EntityDamageRank? damage = appDbContext.DamageRanks.SingleOrDefault(r => r.Uid == uid);
            if (damage != null)
            {
                int avatar = damage.AvatarId;
                double reference = damageRankPartion.MinBy(r => Math.Abs(r.Value - damage.Value))?.Reference ?? 0;

                return new(avatar, damage.Value, reference);
            }
        }

        return null;
    }

    /// <summary>
    /// 获取受到伤害榜
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="scheduleId">计划Id</param>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    public static RankValue? GetTakeDamageRank(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId, string uid)
    {
        List<RankPartion>? damageRankPartion = StatisticsHelper.FromCacheOrDb<List<RankPartion>>(appDbContext, memoryCache, scheduleId, LegacyStatistics.TakeDamageRank);
        if (damageRankPartion != null && damageRankPartion.Any())
        {
            EntityTakeDamageRank? damage = appDbContext.TakeDamageRanks.SingleOrDefault(r => r.Uid == uid);
            if (damage != null)
            {
                int avatar = damage.AvatarId;
                double reference = damageRankPartion.MinBy(r => Math.Abs(r.Value - damage.Value))?.Reference ?? 0;

                return new(avatar, damage.Value, reference);
            }
        }

        return null;
    }
}
