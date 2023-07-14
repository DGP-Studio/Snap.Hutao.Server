// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 成就
/// </summary>
[Table("achievements")]
public sealed class EntityAchievement
{
    /// <summary>
    /// 存档 Id
    /// </summary>
    public long ArchiveId { get; set; }

    /// <summary>
    /// 存档
    /// </summary>
    [ForeignKey(nameof(ArchiveId))]
    public EntityAchievementArchive Archive { get; set; } = default!;

    /// <summary>
    /// Id
    /// </summary>
    [Key]
    public uint Id { get; set; }

    /// <summary>
    /// 当前进度
    /// </summary>
    public uint Current { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AchievementStatus Status { get; set; }
}