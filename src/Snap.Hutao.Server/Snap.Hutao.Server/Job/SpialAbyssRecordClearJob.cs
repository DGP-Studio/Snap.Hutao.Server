// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Quartz;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service;

namespace Snap.Hutao.Server.Job;

/// <summary>
/// 深渊记录清除任务
/// </summary>
public class SpialAbyssRecordClearJob : IJob
{
    private readonly AppDbContext appDbContext;
    private readonly RankService rankService;
    private readonly ILogger<SpialAbyssRecordClearJob> logger;

    /// <summary>
    /// 构造一个新的深渊记录清除任务
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="rankService">排行服务</param>
    /// <param name="logger">日志器</param>
    public SpialAbyssRecordClearJob(AppDbContext appDbContext, RankService rankService, ILogger<SpialAbyssRecordClearJob> logger)
    {
        this.appDbContext = appDbContext;
        this.rankService = rankService;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("触发数据清理", DateTime.Now);

        DateTimeOffset lastAllowed = DateTimeOffset.Now - TimeSpan.FromDays(30);
        long lastAllowedTimestamp = lastAllowed.ToUnixTimeSeconds();

        // 等待 .NET 7
        // https://learn.microsoft.com/zh-cn/ef/core/querying/sql-queries#executing-non-querying-sql

        // 批量删除 长期未提交的记录
        int deletedRecordsCount = await appDbContext.Database
            .ExecuteSqlInterpolatedAsync($"DELETE FROM records WHERE UploadTime < {lastAllowedTimestamp}")
            .ConfigureAwait(false);

        // 批量删除 深渊记录
        int deletedSpiralCount = await appDbContext.Database
            .ExecuteSqlRawAsync("DELETE FROM spiral_abysses")
            .ConfigureAwait(false);

        long removedKeys = await rankService.ClearRanksAsync().ConfigureAwait(false);

        logger.LogInformation("删除了 {count1} 条提交记录 | 删除了 {count2} 条深渊数据 | 删除了 {count3} 个 Redis 键", deletedRecordsCount, deletedSpiralCount, removedKeys);
    }
}