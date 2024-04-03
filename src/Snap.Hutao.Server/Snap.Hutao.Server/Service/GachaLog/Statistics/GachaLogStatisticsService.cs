// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.GachaLog.Statistics;

// Transient
public sealed class GachaLogStatisticsService
{
    public const string Working = "GachaLogStatisticsService.Working";

    // Compile queries that used multiple times to increase performance
    // SELECT DISTINCT "g"."Uid"
    // FROM "GachaItems" AS "g"
    private static readonly Func<AppDbContext, IEnumerable<string>> UidsQuery = EF.CompileQuery((AppDbContext context) =>
        context.GachaItems.AsNoTracking().Select(g => g.Uid).Distinct());

    // .AsQueryable() make sure the compiler use the correct overload.
    // SELECT "g"."UserId", "g"."Uid", "g"."Id", "g"."IsTrusted", "g"."GachaType", "g"."QueryType", "g"."ItemId", "g"."Time"
    // FROM "GachaItems" AS "g"
    // WHERE "g"."Uid" = @__uid_1
    // ORDER BY "g"."Id"
    private static readonly Func<AppDbContext, string, IEnumerable<EntityGachaItem>> GachaItemsQuery = EF.CompileQuery((AppDbContext context, string uid) =>
        context.GachaItems.AsNoTracking().Where(g => g.Uid == uid).OrderBy(g => g.Id).AsQueryable());

    private readonly AppDbContext appDbContext;
    private readonly MetadataDbContext metadataDbContext;
    private readonly DiscordService discordService;
    private readonly IMemoryCache memoryCache;

    public GachaLogStatisticsService(IServiceProvider serviceProvider)
    {
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        metadataDbContext = serviceProvider.GetRequiredService<MetadataDbContext>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    }

    public async Task RunAsync()
    {
        if (memoryCache.TryGetValue(Working, out _))
        {
            return;
        }

        try
        {
            memoryCache.Set(Working, true);
            Map<int, int> idQualityMap = await GetIdQualityMapAsync();
            GachaEventBundle bundle = await GetCurrentGachaEventAsync();

            // If the bundle is not complete, don't run the statistics
            if (bundle is not { AvatarEvent1: { }, AvatarEvent2: { }, WeaponEvent: { } })
            {
                return;
            }

            GachaLogStatisticsTracker tracker = new(idQualityMap, bundle);

            await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
            GachaEventStatistics statistics = tracker.CompleteTracking(appDbContext, memoryCache);
            await discordService.ReportGachaEventStatisticsAsync(statistics).ConfigureAwait(false);
        }
        finally
        {
            memoryCache.Remove(Working);
        }
    }

    private async Task<Map<int, int>> GetIdQualityMapAsync()
    {
        List<KnownItem> knownItems = await metadataDbContext.KnownItems.AsNoTracking().ToListAsync().ConfigureAwait(false);
        return new(knownItems.ToDictionary(x => (int)x.Id, x => (int)x.Quality));
    }

    private async Task<GachaEventBundle> GetCurrentGachaEventAsync()
    {
        DateTime now = DateTime.Now;

        GachaEventInfo? avatarEvent1 = await metadataDbContext.GachaEvents
            .FirstOrDefaultAsync(g => g.From < now && g.To > now && g.Type == GachaConfigType.ActivityAvatar)
            .ConfigureAwait(false);

        GachaEventInfo? avatarEvent2 = await metadataDbContext.GachaEvents
            .FirstOrDefaultAsync(g => g.From < now && g.To > now && g.Type == GachaConfigType.SpeicalActivityAvatar)
            .ConfigureAwait(false);

        GachaEventInfo? weaponEvent = await metadataDbContext.GachaEvents
            .FirstOrDefaultAsync(g => g.From < now && g.To > now && g.Type == GachaConfigType.ActivityWeapon)
            .ConfigureAwait(false);

        GachaEventInfo? chronicled = await metadataDbContext.GachaEvents
            .FirstOrDefaultAsync(g => g.From < now && g.To > now && g.Type == GachaConfigType.ActivityCity)
            .ConfigureAwait(false);

        GachaEventBundle bundle = new()
        {
            AvatarEvent1 = avatarEvent1,
            AvatarEvent2 = avatarEvent2,
            WeaponEvent = weaponEvent,
            Chronicled = chronicled,
        };

        return bundle;
    }

    private void RunCore(GachaLogStatisticsTracker tracker)
    {
        List<string> uids = UidsQuery(appDbContext).ToList();
        HashSet<string> invalidUids = [.. appDbContext.InvalidGachaUids.Select(i => i.Uid)];
        foreach (ref string uid in CollectionsMarshal.AsSpan(uids))
        {
            if (invalidUids.Contains(uid))
            {
                continue;
            }

            tracker.Uid = uid;

            // 按 Id 递增
            List<EntityGachaItem> gachaItems = GachaItemsQuery(appDbContext, uid).ToList();
            tracker.Track(gachaItems);
        }
    }
}