// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Snap.Hutao.Server.Model.GachaLog;

/// <summary>
/// Id集合
/// </summary>
public sealed class EndIds : Dictionary<string, long>
{
    /// <summary>
    /// 查询类型
    /// </summary>
    public static readonly FrozenSet<GachaConfigType> QueryTypes = FrozenSet.ToFrozenSet(
    [
        GachaConfigType.NoviceWish,
        GachaConfigType.StandardWish,
        GachaConfigType.AvatarEventWish,
        GachaConfigType.WeaponEventWish,
    ]);

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="id">id</param>
    public void Add(GachaConfigType type, long id)
    {
        Add($"{type:D}", id);
    }

    public IEnumerable<KeyValuePair<GachaConfigType, long>> Enumerate()
    {
        foreach ((string type, long endId) in this)
        {
            GachaConfigType configType = Enum.Parse<GachaConfigType>(type);
            long exactEndId = endId == 0 ? long.MaxValue : endId;
            yield return new(configType, exactEndId);
        }
    }
}