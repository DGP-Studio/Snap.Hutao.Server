// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Job;

public class LegacyStatisticsRefreshJob : IJob
{
    private readonly StatisticsService statisticsService;

    public LegacyStatisticsRefreshJob(StatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("LegacyStatisticsRefreshJob", CheckInStatus.InProgress);
        try
        {
            await statisticsService.RunAsync().ConfigureAwait(false);
            SentrySdk.CaptureCheckIn("LegacyStatisticsRefreshJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("LegacyStatisticsRefreshJob", CheckInStatus.Error, id);
            throw;
        }
    }
}