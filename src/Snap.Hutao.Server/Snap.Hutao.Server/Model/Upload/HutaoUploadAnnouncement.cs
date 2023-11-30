// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Announcement;

namespace Snap.Hutao.Server.Model.Upload;

public sealed class HutaoUploadAnnouncement
{
    public string Title { get; set; } = default!;

    public string Content { get; set; } = default!;

    public AnnouncementSeverity Severity { get; set; }

    public string Link { get; set; } = default!;

    public string? MaxPresentVersion { get; set; }
}