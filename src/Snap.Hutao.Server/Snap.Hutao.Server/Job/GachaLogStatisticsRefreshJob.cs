// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Service.GachaLog.Statistics;

namespace Snap.Hutao.Server.Job;

public sealed class GachaLogStatisticsRefreshJob : IJob
{
    private readonly GachaLogStatisticsService statisticsService;

    public GachaLogStatisticsRefreshJob(GachaLogStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("GachaLogStatisticsRefreshJob", CheckInStatus.InProgress);
        try
        {
            await statisticsService.RunAsync().ConfigureAwait(false);
            SentrySdk.CaptureCheckIn("GachaLogStatisticsRefreshJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("GachaLogStatisticsRefreshJob", CheckInStatus.Error, id);
            throw;
        }
    }
}