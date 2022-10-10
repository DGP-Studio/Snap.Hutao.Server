// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 记录
/// </summary>
[Table("records")]
public class EntityRecord
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrimaryId { get; set; }

    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = null!;

    /// <summary>
    /// 深渊数据
    /// </summary>
    public EntitySpiralAbyss SpiralAbyss { get; set; } = null!;

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<EntityAvatar> Avatars { get; set; } = null!;
}
