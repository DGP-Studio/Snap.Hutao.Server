// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Job;

public class LegacyStatisticsRefreshJob : IJob
{
    private readonly StatisticsService statisticsService;

    public LegacyStatisticsRefreshJob(StatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return statisticsService.RunAsync();
    }
}