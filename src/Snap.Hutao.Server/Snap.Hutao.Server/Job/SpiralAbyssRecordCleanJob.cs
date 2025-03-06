// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.GachaLog.Statistics;
using Snap.Hutao.Server.Service.Ranking;

namespace Snap.Hutao.Server.Job;

public class SpiralAbyssRecordCleanJob : IJob
{
    private static readonly TimeSpan LongestDaysAllowed = new(30, 0, 0, 0);

    private readonly DiscordService discordService;
    private readonly AppDbContext appDbContext;
    private readonly IRankService rankService;
    private readonly IMemoryCache memoryCache;

    public SpiralAbyssRecordCleanJob(IServiceProvider serviceProvider)
    {
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        rankService = serviceProvider.GetRequiredService<IRankService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("SpiralAbyssRecordCleanJob", CheckInStatus.InProgress);
        try
        {
            try
            {
                memoryCache.Set(GachaLogStatisticsService.Working, true);

                DateTimeOffset lastAllowed = DateTimeOffset.Now - LongestDaysAllowed;
                long lastAllowedTimestamp = lastAllowed.ToUnixTimeSeconds();

                List<long> recordIds = await appDbContext.Records.Where(r => r.UploadTime < lastAllowedTimestamp).Select(r => r.PrimaryId).ToListAsync().ConfigureAwait(false);
                await appDbContext.Avatars.Where(a => recordIds.Contains(a.RecordId)).ExecuteDeleteAsync().ConfigureAwait(false);

                List<long> spiralAbyssIds = await appDbContext.SpiralAbysses.Select(s => s.PrimaryId).ToListAsync().ConfigureAwait(false);
                await appDbContext.DamageRanks.Where(d => spiralAbyssIds.Contains(d.SpiralAbyssId)).ExecuteDeleteAsync().ConfigureAwait(false);
                await appDbContext.TakeDamageRanks.Where(t => spiralAbyssIds.Contains(t.SpiralAbyssId)).ExecuteDeleteAsync().ConfigureAwait(false);
                await appDbContext.SpiralAbyssFloors.Where(s => spiralAbyssIds.Contains(s.SpiralAbyssId)).ExecuteDeleteAsync().ConfigureAwait(false);

                int deletedSpiralAbyssesCount = await appDbContext.SpiralAbysses.ExecuteDeleteAsync().ConfigureAwait(false);
                int deletedRecordsCount = await appDbContext.Records.Where(r => r.UploadTime < lastAllowedTimestamp).ExecuteDeleteAsync().ConfigureAwait(false);

                long removedKeys = await rankService.ClearRanksAsync().ConfigureAwait(false);

                SpiralAbyssRecordCleanResult result = new(deletedRecordsCount, deletedSpiralAbyssesCount, removedKeys);
                await discordService.ReportSpiralAbyssCleanResultAsync(result).ConfigureAwait(false);
            }
            finally
            {
                memoryCache.Remove(GachaLogStatisticsService.Working);
            }

            SentrySdk.CaptureCheckIn("SpiralAbyssRecordCleanJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("SpiralAbyssRecordCleanJob", CheckInStatus.Error, id);
            throw;
        }
    }
}