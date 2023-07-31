// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 受到伤害排行
/// </summary>
[Table("take_damage_ranks")]
[Index(nameof(Value))]
public class EntityTakeDamageRank
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
    public long SpiralAbyssId { get; set; }

    /// <summary>
    /// 引用深渊信息
    /// </summary>
    [ForeignKey(nameof(SpiralAbyssId))]
    public EntitySpiralAbyss SpiralAbyss { get; set; } = default!;

    /// <summary>
    /// 角色Id
    /// </summary>
    public int AvatarId { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Uid
    /// </summary>
    [StringLength(9, MinimumLength = 9)]
    public string Uid { get; set; } = default!;
}