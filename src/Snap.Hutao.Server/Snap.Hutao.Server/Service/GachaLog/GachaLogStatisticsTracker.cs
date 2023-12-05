// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.GachaLog;

public sealed class GachaLogStatisticsTracker
{
    private readonly Map<int, int> idQualityMap;
    private readonly GachaEventInfo currentAvatarEvent1;
    private readonly GachaEventInfo currentAvatarEvent2;
    private readonly GachaEventInfo currentWeaponEvent;
    private readonly FrozenSet<int> currentAvatarEvent1Star5Ids;
    private readonly FrozenSet<int> currentAvatarEvent2Star5Ids;
    private readonly FrozenSet<int> currentWeaponEventStar5Ids;

    private readonly HashSet<string> invalidGachaUids = [];

    // itemId -> count
    private readonly Map<int, long> currentAvatarEvent1Counter = [];
    private readonly Map<int, long> currentAvatarEvent2Counter = [];
    private readonly Map<int, long> currentWeaponEventCounter = [];

    // pulls -> count
    private readonly Map<int, long> avatarEventStar5Distribution = [];
    private readonly Map<int, long> weaponEventStar5Distribution = [];
    private readonly Map<int, long> standardStar5Distribution = [];

    private long totalMixedPullsCounter;
    private long totalAvatarEventValidPullsCounter;
    private long totalWeaponEventValidPullsCounter;
    private long totalStandardValidPullsCounter;

    internal GachaLogStatisticsTracker(Map<int, int> idQualityMap, GachaEventBundle bundle)
    {
        this.idQualityMap = idQualityMap;
        currentAvatarEvent1 = bundle.AvatarEvent1;
        currentAvatarEvent2 = bundle.AvatarEvent2;
        currentWeaponEvent = bundle.WeaponEvent;

        currentAvatarEvent1Star5Ids = currentAvatarEvent1.GetUpOrangeItems().ToFrozenSet();
        currentAvatarEvent2Star5Ids = currentAvatarEvent2.GetUpOrangeItems().ToFrozenSet();
        currentWeaponEventStar5Ids = currentWeaponEvent.GetUpOrangeItems().ToFrozenSet();
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

        foreach (ref readonly EntityGachaItem item in CollectionsMarshal.AsSpan(gachaItems))
        {
            ++totalMixedPullsCounter;

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

    public GachaEventStatistics CompleteTracking(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        if (invalidGachaUids.Count != 0)
        {
            appDbContext.InvalidGachaUids.AddRangeAndSave(invalidGachaUids.Select(uid => new InvalidGachaUid() { Uid = uid }));

            return new()
            {
                Status = GachaEventStatisticsStatus.InvalidItemDetected,
                InvalidUids = invalidGachaUids,
                PullsEnumerated = totalMixedPullsCounter,
            };
        }

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

        GachaEventStatistics statistics = new()
        {
            Status = GachaEventStatisticsStatus.Ok,
            AvatarEvent = currentAvatarEvent1Counter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            AvatarEvent2 = currentAvatarEvent2Counter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            WeaponEvent = currentWeaponEventCounter.Select(kvp => new ItemCount() { Item = kvp.Key, Count = kvp.Value }).ToList(),
            PullsEnumerated = totalMixedPullsCounter,
        };

        SaveStatistics(appDbContext, memoryCache, GachaStatistics.GachaEventStatistics, statistics);
        return statistics;
    }

    private static void SaveStatistics<T>(AppDbContext appDbContext, IMemoryCache memoryCache, string name, T data)
    {
        GachaStatistics? statistics = appDbContext.GachaStatistics
            .SingleOrDefault(s => s.Name == name);

        if (statistics == null)
        {
            statistics = GachaStatistics.CreateWithName(name);
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
                    TryIncreaseCurrentWishItemCounter(item, 3);
                }

                break;
            case 4:
                if (star5Found)
                {
                    ++lastStar5Counter;
                    ++totalPullsCounter;
                    TryIncreaseCurrentWishItemCounter(item, 4);
                }

                break;
            case 5:
                if (star5Found)
                {
                    ++lastStar5Counter;
                    ++totalPullsCounter;
                    TryIncreaseCurrentWishItemCounter(item, 5);

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

    private void TryIncreaseCurrentWishItemCounter(EntityGachaItem item, int quality)
    {
        // TODO: Handle quality 4
        if (currentAvatarEvent1.ItemInThisEvent(item))
        {
            currentAvatarEvent1Counter.IncreaseOne(item.ItemId);

            if (quality is 5 && !(currentAvatarEvent1Star5Ids.Contains(item.ItemId) || SpecializedGachaLogItems.IsAvatarStandardWishItem(item)))
            {
                invalidGachaUids.Add(Uid);
            }
        }
        else if (currentAvatarEvent2.ItemInThisEvent(item))
        {
            currentAvatarEvent2Counter.IncreaseOne(item.ItemId);

            if (quality is 5 && !(currentAvatarEvent2Star5Ids.Contains(item.ItemId) || SpecializedGachaLogItems.IsAvatarStandardWishItem(item)))
            {
                invalidGachaUids.Add(Uid);
            }
        }
        else if (currentWeaponEvent.ItemInThisEvent(item))
        {
            currentWeaponEventCounter.IncreaseOne(item.ItemId);

            if (quality is 5 && !(currentWeaponEventStar5Ids.Contains(item.ItemId) || SpecializedGachaLogItems.IsWeaponStandardWishItem(item)))
            {
                invalidGachaUids.Add(Uid);
            }
        }
    }
}