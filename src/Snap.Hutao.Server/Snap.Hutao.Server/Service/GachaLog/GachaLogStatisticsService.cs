// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
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
    private static readonly Func<AppDbContext, IEnumerable<string>> UidsQuery = EF.CompileQuery((AppDbContext context) =>
        context.GachaItems.AsNoTracking().Select(g => g.Uid).Distinct());

    private static readonly Func<AppDbContext, string, IEnumerable<EntityGachaItem>> GachaItemsQuery = EF.CompileQuery((AppDbContext context, string uid) =>
        context.GachaItems.AsNoTracking().Where(g => g.Uid == uid).OrderBy(g => g.Id));

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;
    private readonly HttpClient httpClient;

    /// <summary>
    /// 创建一个新的祈愿记录统计服务
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="httpClient">Http 客户端</param>
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

        List<GachaEvent> gachaEvents = (await DownloadMetadataAsync<List<GachaEvent>>(GachaEventMetadataUrl).ConfigureAwait(false))!;
        gachaEvents.Sort((x, y) => x.From.CompareTo(y.From));
        GetCurrentGachaEvent(gachaEvents, out GachaEvent avatarEvent1, out GachaEvent avatarEvent2, out GachaEvent weaponEvent);
        GachaLogStatisticsTracker tracker = new(idQualityMap, avatarEvent1, avatarEvent2, weaponEvent);

        using (memoryCache.Flag(Working))
        {
            ValueStopwatch stopwatch = ValueStopwatch.StartNew();
            await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
            tracker.CompleteTracking(appDbContext, memoryCache, stopwatch);
        }
    }

    private static Map<int, int> GetIdQualityMap(List<IdQuality> avatars, List<IdQuality> weapons)
    {
        Map<int, int> idQualityMap = new(avatars.Count + weapons.Count);

        foreach (ref IdQuality item in CollectionsMarshal.AsSpan(avatars))
        {
            ref int quality = ref CollectionsMarshal.GetValueRefOrAddDefault(idQualityMap, item.Id, out _);
            quality = item.Quality;
        }

        foreach (ref IdQuality item in CollectionsMarshal.AsSpan(weapons))
        {
            ref int quality = ref CollectionsMarshal.GetValueRefOrAddDefault(idQualityMap, item.Id, out _);
            quality = item.Quality;
        }

        return idQualityMap;
    }

    private static void GetCurrentGachaEvent(List<GachaEvent> gachaEvents, out GachaEvent avatarEvent1, out GachaEvent avatarEvent2, out GachaEvent weaponEvent)
    {
        gachaEvents.Sort((x, y) => x.From.CompareTo(y.From));

        avatarEvent1 = gachaEvents.Last(g => g.Type == GachaConfigType.AvatarEventWish);
        avatarEvent2 = gachaEvents.Last(g => g.Type == GachaConfigType.AvatarEventWish2);
        weaponEvent = gachaEvents.Last(g => g.Type == GachaConfigType.WeaponEventWish);
    }

    private void RunCore(GachaLogStatisticsTracker tracker)
    {
        List<string> uids = UidsQuery(appDbContext).ToList();
        foreach (ref string uid in CollectionsMarshal.AsSpan(uids))
        {
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