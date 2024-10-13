// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Service.Discord;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.Legacy;

// Transient
public sealed class StatisticsService
{
    /// <summary>
    /// 统计服务正在工作
    /// </summary>
    public const string Working = "StatisticsService.Working";
    private const int Partion = 512;

    // Compile queries that used multiple times to increase performance
    private static readonly Func<AppDbContext, long, IEnumerable<EntityRecord>> PartionQuery = EF.CompileQuery((AppDbContext context, long lastId) =>
        context.Records.AsNoTracking().OrderBy(r => r.PrimaryId).Where(r => r.PrimaryId > lastId).Take(Partion));

    private static readonly Func<AppDbContext, long, IEnumerable<EntityAvatar>> AvatarQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.Avatars.AsNoTracking().Where(a => a.RecordId == recordId));

    private static readonly Func<AppDbContext, long, EntitySpiralAbyss?> SpiralAbyssQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.SpiralAbysses.AsNoTracking().SingleOrDefault(s => s.RecordId == recordId));

    private static readonly Func<AppDbContext, long, IEnumerable<EntityFloor>> FloorQuery = EF.CompileQuery((AppDbContext context, long spiralAbyssId) =>
        context.SpiralAbyssFloors.AsNoTracking().Where(f => f.SpiralAbyssId == spiralAbyssId));

    private readonly SpiralAbyssStatisticsService spiralAbyssStatisticsService;
    private readonly ILogger<StatisticsService> logger;
    private readonly DiscordService discordService;
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public StatisticsService(IServiceProvider serviceProvider)
    {
        spiralAbyssStatisticsService = serviceProvider.GetRequiredService<SpiralAbyssStatisticsService>();
        logger = serviceProvider.GetRequiredService<ILogger<StatisticsService>>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
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

            StatisticsTracker tracker = new();

            ValueStopwatch stopwatch = ValueStopwatch.StartNew();
            await Task.Run(() => RunCore(tracker)).ConfigureAwait(false);
            Overview overview = tracker.CompleteTracking(spiralAbyssStatisticsService, stopwatch);
            await discordService.ReportSpiralAbyssStatisticsAsync(overview).ConfigureAwait(false);
        }
        finally
        {
            memoryCache.Remove(Working);
        }
    }

    private void RunCore(StatisticsTracker tracker)
    {
        int count = 0;
        while (true)
        {
            logger.LogInformation("StatisticsService: Processing {Count} records", count += Partion);

            List<EntityRecord> partialRecords = PartionQuery(appDbContext, tracker.LastId).ToList();
            foreach (ref EntityRecord record in CollectionsMarshal.AsSpan(partialRecords))
            {
                record.Avatars = AvatarQuery(appDbContext, record.PrimaryId).ToList();
                record.SpiralAbyss = SpiralAbyssQuery(appDbContext, record.PrimaryId);

                if (record.SpiralAbyss != null)
                {
                    record.SpiralAbyss.Floors = FloorQuery(appDbContext, record.SpiralAbyss.PrimaryId).ToList();
                }

                tracker.Track(record);
            }

            if (partialRecords.Count < Partion)
            {
                break;
            }
        }
    }
}