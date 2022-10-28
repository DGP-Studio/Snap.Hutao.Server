// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
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
        logger.LogInformation("[{time:yyyy.MM.dd HH:mm:ss.fffffff}] 自动统计数据更新开始", DateTimeOffset.Now);
        await statisticsService.RunAsync().ConfigureAwait(false);
        logger.LogInformation("[{time:yyyy.MM.dd HH:mm:ss.fffffff}] 自动统计数据更新结束", DateTimeOffset.Now);
    }
}