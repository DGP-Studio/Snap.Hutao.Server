// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Achievement;

[Table("achievements")]
public sealed class EntityAchievement
{
    public long ArchiveId { get; set; }

    [ForeignKey(nameof(ArchiveId))]
    public EntityAchievementArchive Archive { get; set; } = default!;

    [Key]
    public uint Id { get; set; }

    public uint Current { get; set; }

    public DateTimeOffset Time { get; set; }

    public AchievementStatus Status { get; set; }
}