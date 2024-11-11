// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Service.GachaLog.Statistics;
using Snap.Hutao.Server.Service.RoleCombat;

namespace Snap.Hutao.Server.Job;

public sealed class RoleCombatStatisticsRefreshJob : IJob
{
    private readonly RoleCombatService statisticsService;

    public RoleCombatStatisticsRefreshJob(RoleCombatService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return statisticsService.RunAsync();
    }
}