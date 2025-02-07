// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.GachaLog;

// Scoped
public sealed class GachaLogService
{
    private const int UidPerUserLimit = 5;

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public GachaLogService(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    public async ValueTask<List<GachaEntry>> GetGachaEntriesForUserAsync(int userId)
    {
        List<GachaEntry> entries = await appDbContext.GachaItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .GroupBy(item => item.Uid)
            .Select(grouping => new GachaEntry()
            {
                Uid = grouping.Key,
                ItemCount = grouping.AsQueryable().Count(),
            })
            .ToListAsync()
            .ConfigureAwait(false);

        List<string> uids = entries.SelectList(entries => entries.Uid);

        HashSet<string> excludedUids = appDbContext.InvalidGachaUids
            .AsNoTracking()
            .Where(uid => uids.Contains(uid.Uid))
            .Select(uid => uid.Uid)
            .AsEnumerable()
            .ToHashSet();

        foreach (GachaEntry entry in entries)
        {
            entry.Excluded = excludedUids.Contains(entry.Uid);
        }

        return entries;
    }

    public async ValueTask<EndIds> GetNewestEndIdsAsync(int userId, string uid)
    {
        EndIds endIds = [];
        foreach (GachaConfigType type in EndIds.QueryTypes)
        {
            EntityGachaItem? item = await appDbContext.GachaItems
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == type)
                .OrderByDescending(i => i.Id)
                .FirstOrDefaultAsync();

            endIds.Add(type, item?.Id ?? 0L);
        }

        return endIds;
    }

    public ValueTask<List<SimpleGachaItem>> GetGachaItemsEarlyThanEndIdsAsync(int userId, UidAndEndIds uidAndEndIds)
    {
        return GetGachaItemsEarlyThanEndIdsAsync(userId, uidAndEndIds.Uid, uidAndEndIds.EndIds);
    }

    public async ValueTask<List<SimpleGachaItem>> GetGachaItemsEarlyThanEndIdsAsync(int userId, string uid, EndIds endIds)
    {
        List<SimpleGachaItem> gachaItems = [];

        foreach ((GachaConfigType configType, long exactEndId) in endIds.EnumerateAsNewest())
        {
            List<EntityGachaItem> items = await appDbContext.GachaItems
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == configType)
                .Where(i => i.Id < exactEndId)
                .ToListAsync()
                .ConfigureAwait(false);

            AppendEntitiesToModels(items, gachaItems);
        }

        return gachaItems;
    }

    public async ValueTask<List<SimpleGachaItem>> GetLimitedGachaItemsEarlyThanEndIdsAsync(int userId, string uid, GachaConfigType configType, long exactEndId, int limit)
    {
        List<SimpleGachaItem> gachaItems = [];

        List<EntityGachaItem> items = await appDbContext.GachaItems
            .AsNoTracking()
            .OrderByDescending(i => i.Id)
            .Where(i => i.UserId == userId)
            .Where(i => i.Uid == uid)
            .Where(i => i.QueryType == configType)
            .Where(i => i.Id < exactEndId)
            .Take(limit)
            .ToListAsync()
            .ConfigureAwait(false);

        AppendEntitiesToModels(items, gachaItems);

        return gachaItems;
    }

    public ValueTask<GachaLogSaveResult> SaveGachaItemsAsync(int userId, UidAndItems uidAndItems)
    {
        return SaveGachaItemsAsync(userId, uidAndItems.Uid, uidAndItems.Items);
    }

    public async ValueTask<GachaLogSaveResult> SaveGachaItemsAsync(int userId, string uid, List<SimpleGachaItem> gachaItems)
    {
        if (await appDbContext.GachaItems.Where(i => i.UserId == userId && i.Uid != uid).Select(i => i.Uid).Distinct().CountAsync().ConfigureAwait(false) >= UidPerUserLimit)
        {
            return new(GachaLogSaveResultKind.UidPerUserLimitExceeded);
        }

        if (!ValidateGachaItems(gachaItems))
        {
            return new(GachaLogSaveResultKind.InvalidGachaItemDetected);
        }

        try
        {
            List<EntityGachaItem> entities = [];
            AppendModelsToEntities(gachaItems, entities, userId, uid, true);
            int count = await appDbContext.GachaItems.AddRangeAndSaveAsync(entities).ConfigureAwait(false);

            return new(GachaLogSaveResultKind.Ok, count);
        }
        catch
        {
            return new(GachaLogSaveResultKind.DatebaseOperationFailed);
        }
    }

    public async ValueTask<int> DeleteGachaItemsAsync(int userId, string uid)
    {
        int count = await appDbContext.GachaItems
            .Where(i => i.UserId == userId)
            .Where(i => i.Uid == uid)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        await appDbContext.InvalidGachaUids.Where(u => u.Uid == uid).ExecuteDeleteAsync().ConfigureAwait(false);
        return count;
    }

    public T? GetGachaLogStatistics<T>(string name)
        where T : class
    {
        if (memoryCache.TryGetValue(name, out object? data))
        {
            return (T)data!;
        }

        GachaStatistics? statistics = appDbContext.GachaStatistics.SingleOrDefault(s => s.Name == name);

        if (statistics == null)
        {
            return null;
        }

        T? tdata = JsonSerializer.Deserialize<T>(statistics.Data);
        return memoryCache.Set(name, tdata);
    }

    private static void AppendEntitiesToModels(List<EntityGachaItem> entities, List<SimpleGachaItem> models)
    {
        foreach (ref readonly EntityGachaItem item in CollectionsMarshal.AsSpan(entities))
        {
            SimpleGachaItem simple = new()
            {
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
                Id = item.Id,
            };

            models.Add(simple);
        }
    }

    private static void AppendModelsToEntities(List<SimpleGachaItem> models, List<EntityGachaItem> entites, int userId, string uid, bool isTrusted)
    {
        foreach (ref SimpleGachaItem item in CollectionsMarshal.AsSpan(models))
        {
            EntityGachaItem entity = new()
            {
                UserId = userId,
                Uid = uid,
                Id = item.Id,
                IsTrusted = isTrusted,
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
            };

            entites.Add(entity);
        }
    }

    private static bool ValidateGachaItems(List<SimpleGachaItem> items)
    {
        foreach (ref readonly SimpleGachaItem item in CollectionsMarshal.AsSpan(items))
        {
            if (item.Id == 0 || item.ItemId == 0)
            {
                return false;
            }
        }

        return true;
    }
}