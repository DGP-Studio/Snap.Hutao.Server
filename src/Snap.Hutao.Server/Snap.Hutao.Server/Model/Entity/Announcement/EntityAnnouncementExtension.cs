// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Announcement;

public static class EntityAnnouncementExtension
{
    public static bool TryGetMaxPresentVersion(this EntityAnnouncement announcement, [NotNullWhen(true)] out Version? version)
    {
        return Version.TryParse(announcement.MaxPresentVersion, out version);
    }
}