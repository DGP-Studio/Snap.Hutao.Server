// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.GachaLog.Statistics;

namespace Snap.Hutao.Server.Job;

public sealed class RoleCombatRecordCleanJob : IJob
{
    private readonly DiscordService discordService;
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public RoleCombatRecordCleanJob(IServiceProvider serviceProvider)
    {
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("RoleCombatRecordCleanJob", CheckInStatus.InProgress);
        try
        {
            try
            {
                memoryCache.Set(GachaLogStatisticsService.Working, true);

                await appDbContext.RoleCombatAvatars.ExecuteDeleteAsync().ConfigureAwait(false);

                int deletedRecordsCount = await appDbContext.RoleCombatRecords.ExecuteDeleteAsync().ConfigureAwait(false);

                RoleCombatRecordCleanResult result = new(deletedRecordsCount);
                await discordService.ReportRoleCombatCleanResultAsync(result).ConfigureAwait(false);
            }
            finally
            {
                memoryCache.Remove(GachaLogStatisticsService.Working);
            }

            SentrySdk.CaptureCheckIn("RoleCombatRecordCleanJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("RoleCombatRecordCleanJob", CheckInStatus.Error, id);
            throw;
        }
    }
}