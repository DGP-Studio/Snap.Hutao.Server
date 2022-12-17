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
    /// 造成伤害
    /// </summary>
    public SimpleRank Damage { get; set; } = default!;

    /// <summary>
    /// 受到伤害
    /// </summary>
    public SimpleRank? TakeDamage { get; set; } = default!;

    /// <summary>
    /// 层
    /// </summary>
    public List<SimpleFloor> Floors { get; set; } = default!;
}