// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

public sealed class GachaEventStatistics
{
    public GachaEventStatisticsStatus Status { get; set; }

    public List<ItemCount> AvatarEvent { get; set; } = default!;

    public List<ItemCount> AvatarEvent2 { get; set; } = default!;

    public List<ItemCount> WeaponEvent { get; set; } = default!;

    public List<ItemCount> Chronicled { get; set; } = default!;

    public HashSet<string> InvalidUids { get; set; } = default!;

    public long PullsEnumerated { get; set; }
}