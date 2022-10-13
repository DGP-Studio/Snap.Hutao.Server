// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Model.Context.Configuration;
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
    public DbSet<RequestStatistics> RequestStatistics { get; set; } = default!;

    /// <summary>
    /// 统计信息
    /// </summary>
    public DbSet<LegacyStatistics> Statistics { get; set; } = default!;

    /// <summary>
    /// 深渊记录
    /// </summary>
    public DbSet<EntityRecord> Records { get; set; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    public DbSet<EntityAvatar> Avatars { get; set; } = default!;

    /// <summary>
    /// 深渊信息
    /// </summary>
    public DbSet<EntitySpiralAbyss> SpiralAbysses { get; set; } = default!;

    /// <summary>
    /// 造成伤害榜
    /// </summary>
    public DbSet<EntityDamageRank> DamageRanks { get; set; } = default!;

    /// <summary>
    /// 受到伤害榜
    /// </summary>
    public DbSet<EntityTakeDamageRank> TakeDamageRanks { get; set; } = default!;

    /// <summary>
    /// 楼层
    /// </summary>
    public DbSet<EntityFloor> SpiralAbyssFloors { get; set; } = default!;

    /// <summary>
    /// 操作锁
    /// </summary>
    public SemaphoreSlim OperationLock { get => operationLock; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EntityFloorConfiguration());
    }
}