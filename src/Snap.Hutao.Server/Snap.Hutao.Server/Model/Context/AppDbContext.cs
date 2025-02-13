// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Snap.Hutao.Server.Model.Context.Configuration;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Entity.Achievement;
using Snap.Hutao.Server.Model.Entity.Announcement;
using Snap.Hutao.Server.Model.Entity.GachaLog;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Entity.RoleCombat;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Entity.Telemetry;

namespace Snap.Hutao.Server.Model.Context;

public sealed class AppDbContext : IdentityDbContext<HutaoUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    #region Infrastructure
    public DbSet<RequestStatistics> RequestStatistics { get; set; } = default!;

    public DbSet<AllowedVersion> AllowedVersions { get; set; } = default!;

    public DbSet<LicenseApplicationRecord> Licenses { get; set; } = default!;

    public DbSet<EntityAnnouncement> Announcements { get; set; } = default!;

    public DbSet<PassportVerification> PassportVerifications { get; set; } = default!;

    public DbSet<GithubIdentity> GithubIdentities { get; set; } = default!;

    public DbSet<RegistrationRecord> RegistrationRecords { get; set; } = default!;

    public DbSet<RedeemCode> RedeemCodes { get; set; } = default!;

    public DbSet<RedeemCodeUseItem> RedeemCodeUseItems { get; set; } = default!;
    #endregion

    #region Telemetry
    public DbSet<HutaoLog> HutaoLogs { get; set; } = default!;

    public DbSet<HutaoLogSingleItem> HutaoLogSingleItems { get; set; } = default!;
    #endregion

    #region SpiralAbyss
    public DbSet<LegacyStatistics> Statistics { get; set; } = default!;

    public DbSet<Banned> BannedList { get; set; } = default!;

    public DbSet<EntityRecord> Records { get; set; } = default!;

    public DbSet<EntityAvatar> Avatars { get; set; } = default!;

    public DbSet<EntitySpiralAbyss> SpiralAbysses { get; set; } = default!;

    public DbSet<EntityFloor> SpiralAbyssFloors { get; set; } = default!;

    public DbSet<EntityDamageRank> DamageRanks { get; set; } = default!;

    public DbSet<EntityTakeDamageRank> TakeDamageRanks { get; set; } = default!;
    #endregion

    #region GachaLog
    public DbSet<GachaStatistics> GachaStatistics { get; set; } = default!;

    public DbSet<InvalidGachaUid> InvalidGachaUids { get; set; } = default!;

    public DbSet<EntityGachaItem> GachaItems { get; set; } = default!;
    #endregion

    #region Achievement WIP
    public DbSet<EntityAchievementArchive> AchievementArchives { get; set; } = default!;

    public DbSet<EntityAchievement> Achievements { get; set; } = default!;
    #endregion

    #region RoleCombat
    public DbSet<RoleCombatRecord> RoleCombatRecords { get; set; } = default!;

    public DbSet<RoleCombatAvatar> RoleCombatAvatars { get; set; } = default!;

    public DbSet<RoleCombatStatistics> RoleCombatStatistics { get; set; } = default!;
    #endregion

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EntityFloorConfiguration());
    }
}