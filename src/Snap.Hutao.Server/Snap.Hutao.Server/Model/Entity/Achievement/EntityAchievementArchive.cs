// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Model.Entity.Achievement;

[Table("achievement_archives")]
public sealed class EntityAchievementArchive
{
    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    public int UserId { get; set; }

    [Key]
    public long Id { get; set; }

    public string Name { get; set; } = default!;
}