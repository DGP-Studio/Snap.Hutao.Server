// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.Legacy;

public class StatisticsTracker
{
    // TODO: 不换队专项统计/一遍过、三间连打统计/总览
    #region Counters

    // 对应层满星 | 出场
    private readonly Map<AvatarId, int> level09PresentCounter = [];
    private readonly Map<AvatarId, int> level10PresentCounter = [];
    private readonly Map<AvatarId, int> level11PresentCounter = [];
    private readonly Map<AvatarId, int> level12PresentCounter = [];

    // 对应层满星 | 持有
    private readonly Map<AvatarId, int> level09RecordwithAvatarCounter = [];
    private readonly Map<AvatarId, int> level10RecordwithAvatarCounter = [];
    private readonly Map<AvatarId, int> level11RecordwithAvatarCounter = [];
    private readonly Map<AvatarId, int> level12RecordwithAvatarCounter = [];

    // 角色命座
    private readonly Map<AvatarId, Map<Constellation, int>> avatarConstellationCounter = [];

    // 角色持有
    private readonly Map<AvatarId, int> avatarHoldingCounter = [];

    // 角色 | 角色搭配
    private readonly Map<AvatarId, Map<AvatarId, int>> avatarAvatarBuildCounter = [];

    // 角色 | 武器搭配
    private readonly Map<AvatarId, Map<WeaponId, int>> avatarWeaponBuildCounter = [];

    // 角色 | 圣遗物搭配
    private readonly Map<AvatarId, Map<ReliquarySets, int>> avatarReliquaryBuildCounter = [];

    // 武器 | 角色搭配
    private readonly Map<WeaponId, Map<AvatarId, int>> wepaonAvatarBuildCounter = [];

    // 对应层满星 | 队伍出场
    private readonly Map<Team, int> level09Battle1TeamCounter = [];
    private readonly Map<Team, int> level09Battle2TeamCounter = [];
    private readonly Map<Team, int> level10Battle1TeamCounter = [];
    private readonly Map<Team, int> level10Battle2TeamCounter = [];
    private readonly Map<Team, int> level11Battle1TeamCounter = [];
    private readonly Map<Team, int> level11Battle2TeamCounter = [];
    private readonly Map<Team, int> level12Battle1TeamCounter = [];
    private readonly Map<Team, int> level12Battle2TeamCounter = [];

    // 仅用于角色数量统计
    private int totalRecordCounter;
    private int totalSpiralAbyssCounter;

    // 对应层满星总记录数
    private int level09TotalRecordCounter;
    private int level10TotalRecordCounter;
    private int level11TotalRecordCounter;
    private int level12TotalRecordCounter;
    #endregion

    #region Specialized Counters
    private int totalSpiralAbyssPassedCounter;
    private int totalSpiralAbyssStarCounter;
    private long totalSpiralAbyssBattleTimesCounter;
    #endregion

    public long LastId { get; private set; } = -1;

    public void Track(EntityRecord record)
    {
        LastId = record.PrimaryId;

        ++totalRecordCounter;
        Map<AvatarId, EntityAvatar> holdingAvatarMap = [];

        // 遍历角色
        foreach (ref readonly EntityAvatar avatar in CollectionsMarshal.AsSpan(record.Avatars))
        {
            // 在循环中顺便填充了 Id -> Entity 映射
            holdingAvatarMap.Add(avatar.AvatarId, avatar);

            // 递增角色持有数
            avatarHoldingCounter.IncreaseOne(avatar.AvatarId);

            // 递增角色命座持有数
            avatarConstellationCounter.GetOrAdd(avatar.AvatarId, Maps.ForConstellation).IncreaseOne(avatar.ActivedConstellationNumber);
        }

        // 仅当深渊记录存在时，下方的统计才有意义
        if (record.SpiralAbyss == null)
        {
            return;
        }

        ++totalSpiralAbyssCounter;
        totalSpiralAbyssBattleTimesCounter += record.SpiralAbyss.TotalBattleTimes;

        // 深渊上场过的角色
        HashSet<AvatarId> recordPresentAvatars = [];

        // 遍历楼层
        foreach (ref readonly EntityFloor floor in CollectionsMarshal.AsSpan(record.SpiralAbyss.Floors))
        {
            totalSpiralAbyssStarCounter += floor.Star;

            if (floor.Index == 12 && floor.Levels.Count == 3)
            {
                ++totalSpiralAbyssPassedCounter;
            }

            // 跳过非满星的数据
            if (floor.Star < 9)
            {
                continue;
            }

            HashSet<AvatarId> floorPresentAvatars = [];

            IncreaseCurrentFloorRecordCount(floor.Index);

            foreach (ref readonly EntityAvatar avatar in CollectionsMarshal.AsSpan(record.Avatars))
            {
                IncreaseCurrentFloorHoldingAvatarCount(floor.Index, avatar.AvatarId);
            }

            foreach (ref readonly SimpleLevel level in CollectionsMarshal.AsSpan(floor.Levels))
            {
                foreach (ref readonly SimpleBattle battle in CollectionsMarshal.AsSpan(level.Battles))
                {
                    Team team = StatisticsTrackerHelper.ToTeam(battle.Avatars);
                    IncreaseCurrentFloorUpDownTeamCount(floor.Index, battle.Index, team);
                    IncreaseAvatarAvatarBuild(team);
                    StatisticsTrackerHelper.AddTeamAvatarToHashSet(team, recordPresentAvatars);
                    StatisticsTrackerHelper.AddTeamAvatarToHashSet(team, floorPresentAvatars);
                }
            }

            foreach (AvatarId avatarId in floorPresentAvatars)
            {
                IncreaseCurrentFloorPresentAvatarCount(floor.Index, avatarId);
            }
        }

        foreach (AvatarId recordAvatarId in recordPresentAvatars)
        {
            EntityAvatar avatar = holdingAvatarMap[recordAvatarId];

            avatarWeaponBuildCounter.GetOrNew(recordAvatarId).IncreaseOne(avatar.WeaponId);
            avatarReliquaryBuildCounter.GetOrNew(recordAvatarId).IncreaseOne(avatar.ReliquarySet);

            wepaonAvatarBuildCounter.GetOrNew(avatar.WeaponId).IncreaseOne(recordAvatarId);
        }
    }

    public Overview CompleteTracking(SpiralAbyssStatisticsService spiralAbyssStatisticsService, ValueStopwatch stopwatch)
    {
        int scheduleId = SpiralAbyssScheduleId.GetForNow();

        // 角色出场率 AvatarAppearanceRank
        {
            List<AvatarAppearanceRank> avatarAppearanceRanks =
            [
                StatisticsTrackerHelper.AvatarAppearanceRank(09, level09PresentCounter, level09TotalRecordCounter),
                StatisticsTrackerHelper.AvatarAppearanceRank(10, level10PresentCounter, level10TotalRecordCounter),
                StatisticsTrackerHelper.AvatarAppearanceRank(11, level11PresentCounter, level11TotalRecordCounter),
                StatisticsTrackerHelper.AvatarAppearanceRank(12, level12PresentCounter, level12TotalRecordCounter),
            ];

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.AvatarAppearanceRank, avatarAppearanceRanks);
        }

        // 角色使用率 AvatarUsageRank
        {
            List<AvatarUsageRank> avatarUsageRanks =
            [
                StatisticsTrackerHelper.AvatarUsageRank(09, level09PresentCounter, level09RecordwithAvatarCounter),
                StatisticsTrackerHelper.AvatarUsageRank(10, level10PresentCounter, level10RecordwithAvatarCounter),
                StatisticsTrackerHelper.AvatarUsageRank(11, level11PresentCounter, level11RecordwithAvatarCounter),
                StatisticsTrackerHelper.AvatarUsageRank(12, level12PresentCounter, level12RecordwithAvatarCounter),
            ];

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.AvatarUsageRank, avatarUsageRanks);
        }

        // AvatarConstellationInfo
        {
            List<AvatarConstellationInfo> avatarConstellationInfos = avatarHoldingCounter.Select(kvp =>
            {
                AvatarId avatar = kvp.Key;
                Map<Constellation, int> constellationCounter = avatarConstellationCounter[avatar];
                return StatisticsTrackerHelper.AvatarConstellationInfo(avatar, totalRecordCounter, kvp.Value, constellationCounter);
            }).ToList();

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.AvatarConstellationInfo, avatarConstellationInfos);
        }

        // AvatarCollocation
        {
            List<AvatarCollocation> avatarCollocations = avatarAvatarBuildCounter.Select(kvp =>
            {
                AvatarId avatar = kvp.Key;

                Map<AvatarId, int> avatarBuildCounter = avatarAvatarBuildCounter[avatar];
                Map<WeaponId, int> weaponBuildCounter = avatarWeaponBuildCounter[avatar];
                Map<ReliquarySets, int> reliquaryBuildCounter = avatarReliquaryBuildCounter[avatar];

                return StatisticsTrackerHelper.AvatarCollocation(avatar, avatarBuildCounter, weaponBuildCounter, reliquaryBuildCounter);
            }).ToList();

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.AvatarCollocation, avatarCollocations);
        }

        // WeaponCollocation
        {
            List<WeaponCollocation> weaponCollocations = wepaonAvatarBuildCounter.Select(kvp =>
            {
                WeaponId weapon = kvp.Key;
                Map<AvatarId, int> avatarBuildCounter = wepaonAvatarBuildCounter[kvp.Key];

                return StatisticsTrackerHelper.WeaponCollocation(weapon, avatarBuildCounter);
            }).ToList();

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.WeaponCollocation, weaponCollocations);
        }

        // 队伍出场 TeamAppearance
        {
            List<TeamAppearance> teamAppearances =
            [
                StatisticsTrackerHelper.TeamAppearance(09, level09Battle1TeamCounter, level09Battle2TeamCounter),
                StatisticsTrackerHelper.TeamAppearance(10, level10Battle1TeamCounter, level10Battle2TeamCounter),
                StatisticsTrackerHelper.TeamAppearance(11, level11Battle1TeamCounter, level11Battle2TeamCounter),
                StatisticsTrackerHelper.TeamAppearance(12, level12Battle1TeamCounter, level12Battle2TeamCounter),
            ];

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.TeamAppearance, teamAppearances);
        }

        // 总览数据 Overview
        {
            double totalTime = stopwatch.GetElapsedTime().TotalMilliseconds;

            Overview overview = new()
            {
                ScheduleId = scheduleId,
                RecordTotal = totalRecordCounter,
                SpiralAbyssTotal = totalSpiralAbyssCounter,
                SpiralAbyssStarTotal = totalSpiralAbyssStarCounter,
                SpiralAbyssBattleTotal = totalSpiralAbyssBattleTimesCounter,
                SpiralAbyssPassed = totalSpiralAbyssPassedCounter,
                SpiralAbyssFullStar = level12TotalRecordCounter,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                TimeTotal = totalTime,
                TimeAverage = totalTime / totalRecordCounter,
            };

            spiralAbyssStatisticsService.SaveStatistics(scheduleId, LegacyStatistics.Overview, overview);

            return overview;
        }
    }

    private void IncreaseCurrentFloorRecordCount(int index)
    {
        switch (index)
        {
            case 09: ++level09TotalRecordCounter; break;
            case 10: ++level10TotalRecordCounter; break;
            case 11: ++level11TotalRecordCounter; break;
            case 12: ++level12TotalRecordCounter; break;
        }
    }

    private void IncreaseCurrentFloorHoldingAvatarCount(int index, AvatarId avatarId)
    {
        // 递增当前层且持有对应角色的记录数
        switch (index)
        {
            case 09: level09RecordwithAvatarCounter.IncreaseOne(avatarId); break;
            case 10: level10RecordwithAvatarCounter.IncreaseOne(avatarId); break;
            case 11: level11RecordwithAvatarCounter.IncreaseOne(avatarId); break;
            case 12: level12RecordwithAvatarCounter.IncreaseOne(avatarId); break;
        }
    }

    private void IncreaseCurrentFloorUpDownTeamCount(int floor, int battle, Team team)
    {
        switch ((floor, battle))
        {
            case (09, 1): level09Battle1TeamCounter.IncreaseOne(team); break;
            case (09, 2): level09Battle2TeamCounter.IncreaseOne(team); break;
            case (10, 1): level10Battle1TeamCounter.IncreaseOne(team); break;
            case (10, 2): level10Battle2TeamCounter.IncreaseOne(team); break;
            case (11, 1): level11Battle1TeamCounter.IncreaseOne(team); break;
            case (11, 2): level11Battle2TeamCounter.IncreaseOne(team); break;
            case (12, 1): level12Battle1TeamCounter.IncreaseOne(team); break;
            case (12, 2): level12Battle2TeamCounter.IncreaseOne(team); break;
        }
    }

    private void IncreaseCurrentFloorPresentAvatarCount(int index, AvatarId avatarId)
    {
        switch (index)
        {
            case 09: level09PresentCounter.IncreaseOne(avatarId); break;
            case 10: level10PresentCounter.IncreaseOne(avatarId); break;
            case 11: level11PresentCounter.IncreaseOne(avatarId); break;
            case 12: level12PresentCounter.IncreaseOne(avatarId); break;
        }
    }

    private void IncreaseAvatarAvatarBuild(Team team)
    {
        if (team.Count == 4)
        {
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position2);
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position3);
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position4);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position1);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position3);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position4);
            avatarAvatarBuildCounter.GetOrNew(team.Position3).IncreaseOne(team.Position1);
            avatarAvatarBuildCounter.GetOrNew(team.Position3).IncreaseOne(team.Position2);
            avatarAvatarBuildCounter.GetOrNew(team.Position3).IncreaseOne(team.Position4);
            avatarAvatarBuildCounter.GetOrNew(team.Position4).IncreaseOne(team.Position1);
            avatarAvatarBuildCounter.GetOrNew(team.Position4).IncreaseOne(team.Position2);
            avatarAvatarBuildCounter.GetOrNew(team.Position4).IncreaseOne(team.Position3);
        }
        else if (team.Count == 3)
        {
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position2);
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position3);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position1);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position3);
            avatarAvatarBuildCounter.GetOrNew(team.Position3).IncreaseOne(team.Position1);
            avatarAvatarBuildCounter.GetOrNew(team.Position3).IncreaseOne(team.Position2);
        }
        else if (team.Count == 2)
        {
            avatarAvatarBuildCounter.GetOrNew(team.Position1).IncreaseOne(team.Position2);
            avatarAvatarBuildCounter.GetOrNew(team.Position2).IncreaseOne(team.Position1);
        }
    }
}