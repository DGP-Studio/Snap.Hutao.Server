// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 深渊数据
/// </summary>
public class SimpleSpiralAbyss
{
    /// <summary>
    /// 计划Id
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// 总战斗次数
    /// </summary>
    public int TotalBattleTimes { get; set; }

    /// <summary>
    /// 总战斗胜利次数
    /// </summary>
    public int TotalWinTimes { get; set; }

    /// <summary>
    /// 造成伤害
    /// </summary>
    public SimpleRank Damage { get; set; } = default!;

    /// <summary>
    /// 击破数
    /// </summary>
    public SimpleRank? Defeat { get; set; }

    /// <summary>
    /// Q 技能数
    /// </summary>
    public SimpleRank? EnergySkill { get; set; }

    /// <summary>
    /// E 技能数
    /// </summary>
    public SimpleRank? NormalSkill { get; set; }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public SimpleRank? TakeDamage { get; set; }

    /// <summary>
    /// 层
    /// </summary>
    public List<SimpleFloor> Floors { get; set; } = default!;
}