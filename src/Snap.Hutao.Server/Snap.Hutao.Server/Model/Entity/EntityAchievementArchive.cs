// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 成就存档
/// </summary>
[Table("achievement_archives")]
public sealed class EntityAchievementArchive
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 存档名称
    /// </summary>
    public string Name { get; set; } = default!;
}