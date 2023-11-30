// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.GachaLog;

/// <summary>
/// 祈愿记录统计服务
/// </summary>
public sealed class GachaLogStatisticsService
{
    /// <summary>
    /// 统计服务正在工作
    /// </summary>
    public const string Working = "GachaLogStatisticsService.Working";
    private const string AvatarMetadataUrl = "https://raw.githubusercontent.com/DGP-Studio/Snap.Metadata/main/Genshin/CHS/Avatar.json";
    private const string WeaponMetadataUrl = "https://raw.githubusercontent.com/DGP-Studio/Snap.Metadata/main/Genshin/CHS/Weapon.json";
    private const string GachaEventMetadataUrl = "https://raw.githubusercontent.com/DGP-Studio/Snap.Metadata/main/Genshin/CHS/GachaEvent.json";

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
    private readonly IMemoryCache memoryCache;
    private readonly HttpClient httpClient;

    public GachaLogStatisticsService(AppDbContext appDbContext, IMemoryCache memoryCache, HttpClient httpClient)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
        this.httpClient = httpClient;
    }

    public async Task RunAsync()
    {
        List<IdQuality> avatars = (await DownloadMetadataAsync<List<IdQuality>>(AvatarMetadataUrl).ConfigureAwait(false))!;
        List<IdQuality> weapons = (await DownloadMetadataAsync<List<IdQuality>>(WeaponMetadataUrl).ConfigureAwait(false))!;

        Map<int, int> idQualityMap = GetIdQualityMap(avatars, weapons);

        List<GachaEventSlim> gachaEvents = (await DownloadMetadataAsync<List<GachaEventSlim>>(GachaEventMetadataUrl).ConfigureAwait(false))!;
        gachaEvents.Sort((x, y) => x.From.CompareTo(y.From));
        GetCurrentGachaEvent(gachaEvents, out GachaEventSlim avatarEvent1, out GachaEventSlim avatarEvent2, out GachaEventSlim weaponEvent);
        GachaLogStatisticsTracker tracker = new(idQualityMap, avatarEvent1, avatarEvent2, weaponEvent);

        using (memoryCache.Flag(Working))
        {
            await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
            tracker.CompleteTracking(appDbContext, memoryCache);
        }
    }

    private static Map<int, int> GetIdQualityMap(List<IdQuality> avatars, List<IdQuality> weapons)
    {
        Map<int, int> idQualityMap = new(avatars.Count + weapons.Count);

        foreach (ref readonly IdQuality item in CollectionsMarshal.AsSpan(avatars))
        {
            ref int quality = ref CollectionsMarshal.GetValueRefOrAddDefault(idQualityMap, item.Id, out _);
            quality = item.Quality;
        }

        foreach (ref readonly IdQuality item in CollectionsMarshal.AsSpan(weapons))
        {
            ref int quality = ref CollectionsMarshal.GetValueRefOrAddDefault(idQualityMap, item.Id, out _);
            quality = item.Quality;
        }

        return idQualityMap;
    }

    private static void GetCurrentGachaEvent(List<GachaEventSlim> gachaEvents, out GachaEventSlim avatarEvent1, out GachaEventSlim avatarEvent2, out GachaEventSlim weaponEvent)
    {
        gachaEvents.Sort((x, y) => x.From.CompareTo(y.From));
        DateTimeOffset now = DateTimeOffset.Now;
        avatarEvent1 = gachaEvents.First(g => g.From < now && g.To > now && g.Type == GachaConfigType.AvatarEventWish);
        avatarEvent2 = gachaEvents.First(g => g.From < now && g.To > now && g.Type == GachaConfigType.AvatarEventWish2);
        weaponEvent = gachaEvents.First(g => g.From < now && g.To > now && g.Type == GachaConfigType.WeaponEventWish);
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

    private Task<T?> DownloadMetadataAsync<T>(string url)
    {
        return httpClient.GetFromJsonAsync<T>(url);
    }
}