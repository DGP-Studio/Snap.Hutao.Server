﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Announcement;
using Snap.Hutao.Server.Model.Upload;
using System.Diagnostics.CodeAnalysis;

namespace Snap.Hutao.Server.Service.Announcement;

// Scoped
public sealed class AnnouncementService
{
    private const string SnapHutaoClientHeaderPrefix = "Snap Hutao/";
    private static readonly TimeSpan MinAnnouncementTimeThreshold = TimeSpan.FromDays(30);

    private readonly AppDbContext appDbContext;

    public AnnouncementService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async ValueTask<List<EntityAnnouncement>> GetAnnouncementsAsync(string? userAgent, string locale, IReadOnlySet<long> excludedIds)
    {
        // Do not return any announcement for non hutao client requests
        if (!TryGetClientVersion(userAgent, out Version? currentVersion))
        {
            return [];
        }

        long limit = (DateTimeOffset.UtcNow - MinAnnouncementTimeThreshold).ToUnixTimeSeconds();

        List<EntityAnnouncement> anns = await appDbContext.Announcements
            .AsNoTracking()
            .OrderByDescending(ann => ann.LastUpdateTime)
            .Where(ann => ann.Locale == locale)
            .Where(ann => ann.LastUpdateTime >= limit)
            .ToListAsync()
            .ConfigureAwait(false);

        List<EntityAnnouncement> result = [];
        foreach (EntityAnnouncement ann in anns)
        {
            if (ann.TryGetMaxPresentVersion(out Version? maxPresentVersion) && maxPresentVersion < currentVersion)
            {
                continue;
            }

            if (!excludedIds.Contains(ann.Id))
            {
                result.Add(ann);
            }
        }

        return result;
    }

    public async ValueTask ProcessUploadAnnouncementAsync(HutaoUploadAnnouncement announcement)
    {
        EntityAnnouncement entity = new()
        {
            Locale = "ALL",
            LastUpdateTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            Title = announcement.Title,
            Content = announcement.Content,
            Severity = announcement.Severity,
            Link = announcement.Link,
            MaxPresentVersion = announcement.MaxPresentVersion,
        };

        appDbContext.Announcements.Add(entity);

        await appDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private static bool TryGetClientVersion(string? userAgent, [NotNullWhen(true)] out Version? version)
    {
        if (string.IsNullOrEmpty(userAgent) || !userAgent.StartsWith(SnapHutaoClientHeaderPrefix))
        {
            version = default;
            return false;
        }

        return Version.TryParse(userAgent[SnapHutaoClientHeaderPrefix.Length..], out version);
    }
}