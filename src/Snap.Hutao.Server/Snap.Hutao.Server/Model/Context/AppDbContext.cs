// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Model.Context;

/// <summary>
/// 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    private readonly SemaphoreSlim operationLock = new(1);

    /// <summary>
    /// 构造一个新的数据库上下文
    /// </summary>
    /// <param name="options">配置</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 请求统计
    /// </summary>
    public DbSet<RequestStatistics> RequestStatistics { get; set; }

    /// <summary>
    /// 操作锁
    /// </summary>
    public SemaphoreSlim OperationLock { get => operationLock; }
}