// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.Metadata;

namespace Snap.Hutao.Server.Service.GachaLog.Statistics;

internal static class SpecializedGachaLogItems
{
    // 10000003 琴
    // 10000016 迪卢克
    // 10000035 七七
    // 10000041 莫娜
    // 10000042 刻晴
    // 10000069 提纳里 3.1 2022/9/28 06:00:00 加入常驻
    // 10000079 迪西雅 3.6 2023/4/12 06:00:00 加入常驻
    // 11501 风鹰剑
    // 11502 天空之刃
    // 12501 天空之傲
    // 12502 狼的末路
    // 13502 天空之脊
    // 13505 和璞鸢
    // 14501 天空之卷
    // 14502 四风原典
    // 15501 天空之翼
    // 15502 阿莫斯之弓
    private static readonly DateTimeOffset MinAllowedTime31 = new(2022, 9, 28, 6, 0, 0, new(8, 0, 0));
    private static readonly DateTimeOffset MinAllowedTime36 = new(2023, 4, 12, 6, 0, 0, new(8, 0, 0));
    private static readonly FrozenSet<int> DefiniteStandardAvatars = FrozenSet.ToFrozenSet([10000003, 10000016, 10000035, 10000041, 10000042]);
    private static readonly FrozenSet<int> DefiniteStandardWeapons = FrozenSet.ToFrozenSet([11501, 11502, 12501, 12502, 13502, 13505, 14501, 14502, 15501, 15502]);

    public static bool IsStandardWishItem(EntityGachaItem item)
    {
        if (item.QueryType is GachaConfigType.ActivityAvatar)
        {
            if (DefiniteStandardAvatars.Contains(item.ItemId))
            {
                return true;
            }

            if (item.ItemId is 10000069 && item.Time > MinAllowedTime31)
            {
                return true;
            }

            if (item.ItemId is 10000079 && item.Time > MinAllowedTime36)
            {
                return true;
            }

            return false;
        }
        else if (item.QueryType is GachaConfigType.ActivityWeapon)
        {
            if (DefiniteStandardWeapons.Contains(item.ItemId))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    public static bool IsAvatarStandardWishItem(EntityGachaItem item)
    {
        if (DefiniteStandardAvatars.Contains(item.ItemId))
        {
            return true;
        }

        if (item.ItemId is 10000069 && item.Time > MinAllowedTime31)
        {
            return true;
        }

        if (item.ItemId is 10000079 && item.Time > MinAllowedTime36)
        {
            return true;
        }

        return false;
    }

    public static bool IsWeaponStandardWishItem(EntityGachaItem item)
    {
        if (DefiniteStandardWeapons.Contains(item.ItemId))
        {
            return true;
        }

        return false;
    }
}