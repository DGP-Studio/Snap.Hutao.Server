// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 深渊数据
/// </summary>
[Table("spiral_abysses")]
public class EntitySpiralAbyss
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    /// <summary>
    /// 外键
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 总战斗次数
    /// </summary>
    public int TotalBattleTimes { get; set; }

    /// <summary>
    /// 总战斗胜利次数
    /// </summary>
    public int TotalWinTimes { get; set; }

    /// <summary>
    /// 引用记录
    /// </summary>
    [ForeignKey(nameof(RecordId))]
    public EntityRecord Record { get; set; } = null!;

    /// <summary>
    /// 造成伤害
    /// </summary>
    public EntityDamageRank Damage { get; set; } = default!;

    /// <summary>
    /// 受到伤害
    /// </summary>
    public EntityTakeDamageRank TakeDamage { get; set; } = default!;

    /// <summary>
    /// 层信息
    /// </summary>
    public List<EntityFloor> Floors { get; set; } = default!;
}
