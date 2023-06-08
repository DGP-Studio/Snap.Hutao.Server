// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

internal sealed class GachaEventStatistics
{
    public List<ItemCount> AvatarEvent { get; set; } = default!;

    public List<ItemCount> AvatarEvent2 { get; set; } = default!;

    public List<ItemCount> WeaponEvent { get; set; } = default!;
}