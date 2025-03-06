// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Service.RoleCombat;

namespace Snap.Hutao.Server.Job;

public sealed class RoleCombatStatisticsRefreshJob : IJob
{
    private readonly RoleCombatService statisticsService;

    public RoleCombatStatisticsRefreshJob(RoleCombatService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("RoleCombatStatisticsRefreshJob", CheckInStatus.InProgress);
        try
        {
            await statisticsService.RunAsync().ConfigureAwait(false);
            SentrySdk.CaptureCheckIn("RoleCombatStatisticsRefreshJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("RoleCombatStatisticsRefreshJob", CheckInStatus.Error, id);
            throw;
        }
    }
}