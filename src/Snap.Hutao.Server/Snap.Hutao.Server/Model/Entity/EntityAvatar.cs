// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 角色信息
/// </summary>
[Table("avatars")]
public class EntityAvatar
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
    /// 引用记录
    /// </summary>
    [ForeignKey(nameof(RecordId))]
    public EntityRecord Record { get; set; } = null!;

    /// <summary>
    /// 角色 Id
    /// </summary>
    public int AvatarId { get; set; }

    /// <summary>
    /// 武器 Id
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// 圣遗物套装Id
    /// </summary>
    [StringLength(50)]
    public string ReliquarySet { get; set; } = default!;

    /// <summary>
    /// 命座
    /// </summary>
    public int ActivedConstellationNumber { get; set; }
}