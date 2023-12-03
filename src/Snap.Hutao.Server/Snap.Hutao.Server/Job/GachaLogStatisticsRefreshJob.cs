// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Service.GachaLog;

namespace Snap.Hutao.Server.Job;

public class GachaLogStatisticsRefreshJob : IJob
{
    private readonly GachaLogStatisticsService statisticsService;

    public GachaLogStatisticsRefreshJob(GachaLogStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return statisticsService.RunAsync();
    }
}