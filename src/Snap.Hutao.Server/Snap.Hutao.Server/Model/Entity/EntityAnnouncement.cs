﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

[Table("announcements")]
[PrimaryKey(nameof(Id), nameof(Locale))]
public sealed class EntityAnnouncement
{
    /// <summary>
    /// Id
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string Locale { get; set; } = default!;

    /// <summary>
    /// 最后更新日期
    /// </summary>
    public long LastUpdateTime { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// 严重度
    /// </summary>
    public AnnouncementSeverity Severity { get; set; }

    /// <summary>
    /// 原帖链接
    /// </summary>
    public string Link { get; set; } = default!;
}