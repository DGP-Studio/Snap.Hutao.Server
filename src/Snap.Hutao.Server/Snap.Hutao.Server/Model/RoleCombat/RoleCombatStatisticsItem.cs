// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Legacy;

namespace Snap.Hutao.Server.Model.RoleCombat;

public sealed class RoleCombatStatisticsItem
{
    public int RecordTotal { get; set; }

    public List<ItemRate<uint, double>> BackupAvatarRates { get; set; } = default!;
}