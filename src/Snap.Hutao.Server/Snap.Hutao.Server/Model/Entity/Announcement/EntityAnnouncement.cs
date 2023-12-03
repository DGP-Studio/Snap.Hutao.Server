// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel;

namespace Snap.Hutao.Server.Model.Entity.Announcement;

[Table("announcements")]
[PrimaryKey(nameof(Id), nameof(Locale))]
public sealed class EntityAnnouncement
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string Locale { get; set; } = default!;

    public long LastUpdateTime { get; set; }

    public string Title { get; set; } = default!;

    public string Content { get; set; } = default!;

    public AnnouncementSeverity Severity { get; set; }

    public string Link { get; set; } = default!;

    public AnnouncementKind Kind { get; set; }

    public string? MaxPresentVersion { get; set; }
}