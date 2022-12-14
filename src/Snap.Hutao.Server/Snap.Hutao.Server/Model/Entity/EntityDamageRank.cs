// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 造成伤害排行
/// </summary>
[Table("damage_ranks")]
[Index(nameof(Value))]
public class EntityDamageRank
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
    public string Uid { get; set; } = default!;
}