// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Legacy;

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
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    public static ItemRate<int, double> GetDamageRank(AppDbContext appDbContext, string uid)
    {
        List<IndexUidAvatarValue> rank = appDbContext.DamageRanks
            .OrderBy(r => r.Value)
            .AsEnumerable()
            .Select((r, i) => new IndexUidAvatarValue(i, r.Uid, r.AvatarId, r.Value))
            .ToList();

        int totalCount = rank.Count;
        IndexUidAvatarValue item = rank.First(r => r.Uid == uid);
        int index = item.Index + 1;
        return new(item.AvatarId, index / (double)totalCount);
    }

    /// <summary>
    /// 获取受到伤害榜
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="uid">uid</param>
    /// <returns>排行</returns>
    public static ItemRate<int, double> GetTakeDamageRank(AppDbContext appDbContext, string uid)
    {
        List<IndexUidAvatarValue> rank = appDbContext.TakeDamageRanks
            .OrderBy(r => r.Value)
            .AsEnumerable()
            .Select((r, i) => new IndexUidAvatarValue(i, r.Uid, r.AvatarId, r.Value))
            .ToList();

        int totalCount = rank.Count;
        IndexUidAvatarValue item = rank.First(r => r.Uid == uid);
        int index = item.Index + 1;
        return new(item.AvatarId, index / (double)totalCount);
    }

    private struct IndexUidAvatarValue
    {
        public int Index;
        public string Uid;
        public int AvatarId;
        public int Value;

        public IndexUidAvatarValue(int index, string uid, int avatar, int value)
        {
            Index = index;
            Uid = uid;
            AvatarId = avatar;
            Value = value;
        }
    }
}
