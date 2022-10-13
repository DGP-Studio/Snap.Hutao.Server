// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Job;

/// <summary>
/// 统计刷新任务
/// </summary>
public class LegacyStatisticsRefreshJob : IJob
{
    private readonly StatisticsService statisticsService;
    private readonly ILogger<LegacyStatisticsRefreshJob> logger;

    /// <summary>
    /// 构造一个新的统计刷新任务
    /// </summary>
    /// <param name="statisticsService">统计服务</param>
    /// <param name="logger">日志器</param>
    public LegacyStatisticsRefreshJob(StatisticsService statisticsService, ILogger<LegacyStatisticsRefreshJob> logger)
    {
        this.statisticsService = statisticsService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("已触发数据更新...");
        ValueStopwatch stopwatch = ValueStopwatch.StartNew();
        await statisticsService.RunAsync().ConfigureAwait(false);
        logger.LogInformation("数据更新完成，耗时 {time}ms", stopwatch.GetElapsedTime().TotalMilliseconds);
    }
}