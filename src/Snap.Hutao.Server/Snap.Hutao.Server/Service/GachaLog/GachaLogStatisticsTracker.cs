// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.GachaLog;

public sealed class GachaLogStatisticsTracker
{
    private readonly Map<int, int> idQualityMap;
    private readonly GachaEvent currentAvatarEvent1;
    private readonly GachaEvent currentAvatarEvent2;
    private readonly GachaEvent currentWeaponEvent;

    private readonly HashSet<string> invalidGachaUids = new();

    // itemId -> count
    private readonly Map<int, long> currentAvatarEvent1Counter = new();
    private readonly Map<int, long> currentAvatarEvent2Counter = new();
    private readonly Map<int, long> currentWeaponEventCounter = new();

    // pulls -> count
    private readonly Map<int, long> avatarEventStar5Distribution = new();
    private readonly Map<int, long> weaponEventStar5Distribution = new();
    private readonly Map<int, long> standardStar5Distribution = new();

    private long totalAvatarEventValidPullsCounter;
    private long totalWeaponEventValidPullsCounter;
    private long totalStandardValidPullsCounter;

    internal GachaLogStatisticsTracker(Map<int, int> idQualityMap, GachaEvent currentAvatarEvent1, GachaEvent currentAvatarEvent2, GachaEvent currentWeaponEvent)
    {
        this.idQualityMap = idQualityMap;
        this.currentAvatarEvent1 = currentAvatarEvent1;
        this.currentAvatarEvent2 = currentAvatarEvent2;
        this.currentWeaponEvent = currentWeaponEvent;
    }

    /// <summary>
    /// 当前处理的 Uid
    /// </summary>
    public string Uid { get; set; } = string.Empty;

    /// <summary>
    /// 追踪记录
    /// </summary>
    /// <param name="gachaItems">祈愿记录</param>
    public void Track(List<EntityGachaItem> gachaItems)
    {
        bool avatarEventFirstStar5Found = false;
        bool weaponEventFirstStar5Found = false;
        bool standardFirstStar5Found = false;

        int currentAvatarEventCountToLastStar5 = 0;
        int currentWeaponEventCountToLastStar5 = 0;
        int currentStandardCountToLastStar5 = 0;

        foreach (ref EntityGachaItem item in CollectionsMarshal.AsSpan(gachaItems))
        {
            switch (item.QueryType)
            {
                case GachaConfigType.AvatarEventWish:
                    TrackForSpecficQueryTypeWish(
                        item,
                        ref avatarEventFirstStar5Found,
                        ref currentAvatarEventCountToLastStar5,
                        ref totalAvatarEventValidPullsCounter,
                        avatarEventStar5Distribution,
                        90);
                    break;
                case GachaConfigType.WeaponEventWish:
                    TrackForSpecficQueryTypeWish(
                        item,
                        ref weaponEventFirstStar5Found,
                        ref currentWeaponEventCountToLastStar5,
                        ref totalWeaponEventValidPullsCounter,
                        weaponEventStar5Distribution,
                        80);
                    break;
                case GachaConfigType.StandardWish:
                    TrackForSpecficQueryTypeWish(
                        item,
                        ref standardFirstStar5Found,
                        ref currentStandardCountToLastStar5,
                        ref totalStandardValidPullsCounter,
                        standardStar5Distribution,
                        90);
                    break;
            }
        }
    }

    public void CompleteTracking(AppDbContext appDbContext, IMemoryCache memoryCache, ValueStopwatch stopwatch)
    {
        GachaDistribution avatarEventDistribution = new()
        {
            TotalValidPulls = totalAvatarEventValidPullsCounter,
            Distribution = avatarEventStar5Distribution.Select(kvp => new PullCount() { Pull = kvp.Key, Count = kvp.Value }).ToList(),
        };

        SaveStatistics(appDbContext, memoryCache, GachaStatistics.AvaterEventGachaDistribution, avatarEventDistribution);

        GachaDistribution weaponEventDistribution = new()
        {
            TotalValidPulls = totalWeaponEventValidPullsCounter,
            Distribution = weaponEventStar5Distribution.Select(kvp => new PullCount() { Pull = kvp.Key, Count = kvp.Value }).ToList(),
        };

        SaveStatistics(appDbContext, memoryCache, GachaStatistics.WeaponEventGachaDistribution, weaponEventDistribution);

        GachaDistribution standardDistribution = new()
        {
            TotalValidPulls = totalStandardValidPullsCounter,
            Distribution = standardStar5Distribution.Select(kvp => new PullCount() { Pull = kvp.Key, Count = kvp.Value }).ToList(),
        };

        SaveStatistics(appDbContext, memoryCache, GachaStatistics.StandardGachaDistribution, standardDistribution);

        GachaEventStatistics gachaEventStatistics = new()
        {
            AvatarEvent = currentAvatarEvent1Counter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            AvatarEvent2 = currentAvatarEvent2Counter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            WeaponEvent = currentWeaponEventCounter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            InvalidUids = invalidGachaUids,
        };

        SaveStatistics(appDbContext, memoryCache, GachaStatistics.GachaEventStatistics, gachaEventStatistics);
    }

    private static void SaveStatistics<T>(AppDbContext appDbContext, IMemoryCache memoryCache, string name, T data)
    {
        GachaStatistics? statistics = appDbContext.GachaStatistics
                .SingleOrDefault(s => s.Name == name);

        if (statistics == null)
        {
            statistics = GachaStatistics.Create(name);
            appDbContext.GachaStatistics.Add(statistics);
        }

        memoryCache.Set(name, data);
        statistics.Data = JsonSerializer.Serialize(data);

        appDbContext.SaveChanges();
    }

    private void TrackForSpecficQueryTypeWish(EntityGachaItem item, ref bool star5Found, ref int lastStar5Counter, ref long totalPullsCounter, Map<int, long> distribution, int pullMaxThreshold)
    {
        switch (idQualityMap[item.ItemId])
        {
            case 3:
                if (star5Found)
                {
                    ++lastStar5Counter;
                    ++totalPullsCounter;
                    TryIncreaseCurrentWishItemCounter(item);
                }

                break;
            case 4:
                if (star5Found)
                {
                    ++lastStar5Counter;
                    ++totalPullsCounter;
                    TryIncreaseCurrentWishItemCounter(item);
                }

                break;
            case 5:
                if (star5Found)
                {
                    ++lastStar5Counter;
                    ++totalPullsCounter;
                    TryIncreaseCurrentWishItemCounter(item);

                    if (lastStar5Counter > pullMaxThreshold)
                    {
                        invalidGachaUids.Add(Uid);
                    }

                    distribution.IncreaseOne(lastStar5Counter);

                    lastStar5Counter = 0; // reset
                }

                star5Found = true;
                break;
        }
    }

    private void TryIncreaseCurrentWishItemCounter(EntityGachaItem item)
    {
        if (item.GachaType == GachaConfigType.AvatarEventWish && item.Time >= currentAvatarEvent1.From && item.Time <= currentAvatarEvent1.To)
        {
            currentAvatarEvent1Counter.IncreaseOne(item.ItemId);
        }
        else if (item.GachaType == GachaConfigType.AvatarEventWish2 && item.Time >= currentAvatarEvent2.From && item.Time <= currentAvatarEvent2.To)
        {
            currentAvatarEvent2Counter.IncreaseOne(item.ItemId);
        }
        else if (item.GachaType == GachaConfigType.WeaponEventWish && item.Time >= currentWeaponEvent.From && item.Time <= currentWeaponEvent.To)
        {
            currentWeaponEventCounter.IncreaseOne(item.ItemId);
        }
    }
}