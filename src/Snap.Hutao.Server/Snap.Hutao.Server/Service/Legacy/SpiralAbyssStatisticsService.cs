// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;

namespace Snap.Hutao.Server.Service.Legacy;

// Scoped
public sealed class SpiralAbyssStatisticsService
{
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public SpiralAbyssStatisticsService(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    public T? GetStatistics<T>(string name)
        where T : class
    {
        return GetStatistics<T>(name, SpiralAbyssScheduleId.GetForNow());
    }

    public T? GetStatistics<T>(string name, int scheduleId)
        where T : class
    {
        string key = $"{name}:{scheduleId}";
        if (memoryCache.TryGetValue(key, out object? data))
        {
            return (T)data!;
        }

        LegacyStatistics? statistics = appDbContext.Statistics
            .Where(s => s.ScheduleId == scheduleId)
            .SingleOrDefault(s => s.Name == name);

        if (statistics is null)
        {
            return null;
        }

        T? typedData = JsonSerializer.Deserialize<T>(statistics.Data);
        memoryCache.Set(key, typedData, TimeSpan.FromDays(1));

        return typedData;
    }

    public void SaveStatistics<T>(int scheduleId, string name, T data)
    {
        LegacyStatistics? statistics = appDbContext.Statistics
            .Where(s => s.ScheduleId == scheduleId)
            .SingleOrDefault(s => s.Name == name);

        if (statistics == null)
        {
            statistics = LegacyStatistics.CreateWithNameAndScheduleId(name, scheduleId);
            appDbContext.Statistics.Add(statistics);
        }

        memoryCache.Set($"{name}:{scheduleId}", data, TimeSpan.FromDays(1));
        statistics.Data = JsonSerializer.Serialize(data);

        appDbContext.SaveChanges();
    }
}
