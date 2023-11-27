// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

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

    /// <summary>
    /// 公告类型
    /// </summary>
    public AnnouncementKind Kind { get; set; }

    /// <summary>
    /// 最高的呈现版本
    /// 不会在高于此版本的请求中返回此公告
    /// </summary>
    public string? MaxPresentVersion { get; set; }

    public bool TryGetMaxPresentVersion([NotNullWhen(true)] out Version? version)
    {
        return Version.TryParse(MaxPresentVersion, out version);
    }
}