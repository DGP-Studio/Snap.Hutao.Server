// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Quartz;
using Snap.Hutao.Server.Model.Context;

namespace Snap.Hutao.Server.Job;

/// <summary>
/// 胡桃日志清理任务
/// </summary>
public class HutaoLogCleanJob : IJob
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 构造一个新的胡桃日志清理任务
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    public HutaoLogCleanJob(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        DateTimeOffset lastAllowed = DateTimeOffset.Now - TimeSpan.FromDays(30);
        long lastAllowedTimestamp = lastAllowed.ToUnixTimeSeconds();

        await appDbContext.Database
            .ExecuteSqlInterpolatedAsync($"DELETE FROM hutao_logs WHERE UploadTime < {lastAllowedTimestamp}")
            .ConfigureAwait(false);
    }
}