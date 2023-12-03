// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleSpiralAbyss
{
    public int ScheduleId { get; set; }

    public int TotalBattleTimes { get; set; }

    public int TotalWinTimes { get; set; }

    public SimpleRank Damage { get; set; } = default!;

    public SimpleRank? Defeat { get; set; }

    public SimpleRank? EnergySkill { get; set; }

    public SimpleRank? NormalSkill { get; set; }

    public SimpleRank? TakeDamage { get; set; }

    public List<SimpleFloor> Floors { get; set; } = default!;
}