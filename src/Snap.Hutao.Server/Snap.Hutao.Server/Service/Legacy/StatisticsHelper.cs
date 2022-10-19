// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Text.Json;

namespace Snap.Hutao.Server.Service.Legacy;

/// <summary>
/// 统计帮助类
/// </summary>
public static class StatisticsHelper
{
    /// <summary>
    /// 创建角色出场率
    /// </summary>
    /// <param name="floor">层</param>
    /// <param name="counter">出场总数</param>
    /// <param name="recordCounter">持有角色记录数</param>
    /// <returns>角色出场率</returns>
    public static AvatarUsageRank AvatarUsageRank(int floor, Map<AvatarId, int> counter, Map<AvatarId, int> recordCounter)
    {
        return new()
        {
            Floor = floor,
            Ranks = counter.Select(kvp => new ItemRate<int, double>(kvp.Key, kvp.Value / (double)recordCounter[kvp.Key])).ToList(),
        };
    }

    /// <summary>
    /// 创建角色出场率
    /// </summary>
    /// <param name="floor">层</param>
    /// <param name="counter">出场总数</param>
    /// <param name="levelRecordCount">当前楼层记录数</param>
    /// <returns>角色出场率</returns>
    public static AvatarAppearanceRank AvatarAppearanceRank(int floor, Map<AvatarId, int> counter, int levelRecordCount)
    {
        return new()
        {
            Floor = floor,
            Ranks = counter.Select(kvp => new ItemRate<int, double>(kvp.Key, kvp.Value / (double)levelRecordCount)).ToList(),
        };
    }

    /// <summary>
    /// 构造一个新的角色持有率信息
    /// </summary>
    /// <param name="avatarId">角色Id</param>
    /// <param name="totalRecord">总记录数</param>
    /// <param name="avatarCount">角色个数</param>
    /// <param name="constellationCounter">命座计数器</param>
    /// <returns>角色持有率信息</returns>
    public static AvatarConstellationInfo AvatarConstellationInfo(AvatarId avatarId, int totalRecord, int avatarCount, Map<Constellation, int> constellationCounter)
    {
        return new(avatarId)
        {
            HoldingRate = (double)avatarCount / totalRecord,
            Constellations = constellationCounter.Select(kvp => new ItemRate<int, double>(kvp.Key, kvp.Value / (double)avatarCount)).ToList(),
        };
    }

    /// <summary>
    /// 构造一个新的角色搭配
    /// </summary>
    /// <param name="avatarId">角色Id</param>
    /// <param name="avatarBuildCounter">角色构筑计数器</param>
    /// <param name="weaponBuildCounter">武器构筑计数器</param>
    /// <param name="reliquaryBuildCounter">圣遗物构筑计数器</param>
    /// <returns>角色搭配</returns>
    public static AvatarCollocation AvatarCollocation(AvatarId avatarId, Map<AvatarId, int> avatarBuildCounter, Map<WeaponId, int> weaponBuildCounter, Map<ReliquarySets, int> reliquaryBuildCounter)
    {
        double coAvatarTotalCount = avatarBuildCounter.Sum(kvp => kvp.Value);
        double weaponTotalCount = weaponBuildCounter.Sum(kvp => kvp.Value);
        double reliquarySetTotalCount = reliquaryBuildCounter.Sum(kvp => kvp.Value);

        return new(avatarId)
        {
            Avatars = avatarBuildCounter
                .OrderByDescending(x => x.Value)
                .Take(8)
                .Select(kvp => new ItemRate<int, double>(kvp.Key, kvp.Value / coAvatarTotalCount))
                .ToList(),
            Weapons = weaponBuildCounter
                .OrderByDescending(x => x.Value)
                .Take(8)
                .Select(kvp => new ItemRate<int, double>(kvp.Key, kvp.Value / weaponTotalCount))
                .ToList(),
            Reliquaries = reliquaryBuildCounter
                .OrderByDescending(x => x.Value)
                .Take(8)
                .Select(kvp => new ItemRate<string, double>(kvp.Key, kvp.Value / reliquarySetTotalCount))
                .ToList(),
        };
    }

    /// <summary>
    /// 构造一个新的队伍出场
    /// </summary>
    /// <param name="floor">层</param>
    /// <param name="upTeamCounter">上半</param>
    /// <param name="downTeamCounter">下半</param>
    /// <returns>队伍出场</returns>
    public static TeamAppearance TeamAppearance(int floor, Map<Team, int> upTeamCounter, Map<Team, int> downTeamCounter)
    {
        return new()
        {
            Floor = floor,
            Up = upTeamCounter
                .OrderByDescending(x => x.Value)
                .Take(16)
                .Select(kvp => new ItemRate<string, int>(kvp.Key, kvp.Value))
                .ToList(),
            Down = downTeamCounter
                .OrderByDescending(x => x.Value)
                .Take(16)
                .Select(kvp => new ItemRate<string, int>(kvp.Key, kvp.Value))
                .ToList(),
        };
    }

    /// <summary>
    /// 转换到队伍
    /// </summary>
    /// <param name="avatars">角色</param>
    /// <returns>队伍</returns>
    public static Team AsTeam(List<int> avatars)
    {
        return new(avatars);
    }

    /// <summary>
    /// 保存统计数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="scheduleId">计划Id</param>
    /// <param name="name">名称</param>
    /// <param name="data">数据</param>
    public static void SaveStatistics<T>(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId, string name, T data)
    {
        LegacyStatistics? statistics = appDbContext.Statistics
                .Where(s => s.ScheduleId == scheduleId)
                .SingleOrDefault(s => s.Name == name);

        if (statistics == null)
        {
            statistics = LegacyStatistics.Create(name, scheduleId);
            appDbContext.Statistics.Add(statistics);
        }

        memoryCache.Set(name, data);
        statistics.Data = JsonSerializer.Serialize(data);

        appDbContext.SaveChanges();
    }

    /// <summary>
    /// 从缓存或数据库中获取值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="scheduleId">规划Id</param>
    /// <param name="name">名称</param>
    /// <returns>值</returns>
    public static T? FromCacheOrDb<T>(AppDbContext appDbContext, IMemoryCache memoryCache, int scheduleId, string name)
        where T : class
    {
        if (memoryCache.TryGetValue(name, out object? data))
        {
            return (T)data;
        }

        LegacyStatistics? statistics = appDbContext.Statistics
            .Where(s => s.ScheduleId == scheduleId)
            .SingleOrDefault(s => s.Name == name);

        if (statistics != null)
        {
            T? tdata = JsonSerializer.Deserialize<T>(statistics.Data);
            memoryCache.Set(name, tdata);

            return tdata;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 获取深渊期数
    /// </summary>
    /// <returns>当前深渊期数</returns>
    public static int GetScheduleId()
    {
        return GetScheduleId(DateTimeOffset.Now);
    }

    /// <summary>
    /// 获取深渊期数
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns>深渊期数</returns>
    public static int GetScheduleId(DateTimeOffset time)
    {
        int periodNum = (((time.Year - 2020) * 12) + time.Month) * 2;

        TimeSpan fourHours = TimeSpan.FromHours(4);

        // 上半月
        if (time.Day < 16 || (time.Day == 16 && ((time - time.Date) < fourHours)))
        {
            periodNum--;
        }

        // 上个月
        if (time.Day == 1 && ((time - time.Date) < fourHours))
        {
            periodNum--;
        }

        return periodNum - 12;
    }
}
