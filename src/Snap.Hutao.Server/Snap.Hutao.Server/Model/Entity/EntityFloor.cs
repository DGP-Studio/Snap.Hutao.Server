// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 层信息
/// </summary>
[Table("spiral_abysses_floors")]
public class EntityFloor
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
    /// 层编号 1-12|9-12
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 星数
    /// </summary>
    public int Star { get; set; }

    /// <summary>
    /// Json !!!
    /// 间信息
    /// </summary>
    public List<SimpleLevel> Levels { get; set; } = default!;
}