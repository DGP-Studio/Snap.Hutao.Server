// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Snap.Hutao.Server.Model.Context.Configuration;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Model.Context;

/// <summary>
/// 数据库上下文
/// </summary>
public class AppDbContext : IdentityDbContext<HutaoUser, IdentityRole<int>, int>
{
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
    /// 祈愿统计信息
    /// </summary>
    public DbSet<GachaStatistics> GachaStatistics { get; set; } = default!;

    /// <summary>
    /// 封禁的用户列表
    /// </summary>
    public DbSet<Banned> BannedList { get; set; } = default!;

    /// <summary>
    /// 无效的祈愿记录Uid
    /// </summary>
    public DbSet<InvalidGachaUid> InvalidGachaUids { get; set; } = default!;

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
    /// 胡桃日志
    /// </summary>
    public DbSet<HutaoLog> HutaoLogs { get; set; } = default!;

    /// <summary>
    /// 单个日志
    /// </summary>
    public DbSet<HutaoLogSingleItem> HutaoLogSingleItems { get; set; } = default!;

    /// <summary>
    /// 祈愿记录物品
    /// </summary>
    public DbSet<EntityGachaItem> GachaItems { get; set; } = default!;

    /// <summary>
    /// 成就存档
    /// </summary>
    public DbSet<EntityAchievementArchive> AchievementArchives { get; set; } = default!;

    /// <summary>
    /// 成就
    /// </summary>
    public DbSet<EntityAchievement> Achievements { get; set; } = default!;

    /// <summary>
    /// 许可证记录
    /// </summary>
    public DbSet<LicenseApplicationRecord> Licenses { get; set; } = default!;

    /// <summary>
    /// 公告
    /// </summary>
    public DbSet<EntityAnnouncement> Announcements { get; set; } = default!;

    /// <summary>
    /// 允许的版本头
    /// </summary>
    public DbSet<AllowedVersion> AllowedVersions { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EntityFloorConfiguration());
    }
}
