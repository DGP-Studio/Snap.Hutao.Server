// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;

namespace Snap.Hutao.Server.Model.GachaLog;

public sealed class EndIds : Dictionary<string, long>
{
    public static readonly FrozenSet<GachaConfigType> QueryTypes = FrozenSet.ToFrozenSet(
    [
        GachaConfigType.NoviceWish,
        GachaConfigType.StandardWish,
        GachaConfigType.AvatarEventWish,
        GachaConfigType.WeaponEventWish,
    ]);

    public void Add(GachaConfigType type, long id)
    {
        Add($"{type:D}", id);
    }

    public IEnumerable<KeyValuePair<GachaConfigType, long>> EnumerateAsNewest()
    {
        foreach ((string type, long endId) in this)
        {
            GachaConfigType configType = Enum.Parse<GachaConfigType>(type);
            long exactEndId = endId == 0 ? long.MaxValue : endId;
            yield return new(configType, exactEndId);
        }
    }
}