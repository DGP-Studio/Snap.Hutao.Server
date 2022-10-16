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
    public long PrimaryId { get; set; }

    /// <summary>
    /// Uid
    /// </summary>
    [StringLength(9, MinimumLength =9)]
    public string Uid { get; set; } = null!;

    /// <summary>
    /// 上传方
    /// </summary>
    public string Uploader { get; set; } = null!;

    /// <summary>
    /// 上传时间戳
    /// </summary>
    public long UploadTime { get; set; }

    /// <summary>
    /// 深渊数据
    /// </summary>
    public EntitySpiralAbyss? SpiralAbyss { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<EntityAvatar> Avatars { get; set; } = null!;
}