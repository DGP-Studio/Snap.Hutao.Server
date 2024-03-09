// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;

namespace Snap.Hutao.Server.Service.GachaLog.Statistics;

internal sealed class GachaEventBundle
{
    public GachaEventInfo? AvatarEvent1 { get; set; } = default!;

    public GachaEventInfo? AvatarEvent2 { get; set; } = default!;

    public GachaEventInfo? WeaponEvent { get; set; } = default!;

    public GachaEventInfo? Chronicled { get; set; } = default!;
}