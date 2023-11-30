// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.SpiralAbyss;

[Table("spiral_abysses")]
public class EntitySpiralAbyss
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public long RecordId { get; set; }

    public int TotalBattleTimes { get; set; }

    public int TotalWinTimes { get; set; }

    [ForeignKey(nameof(RecordId))]
    public EntityRecord Record { get; set; } = null!;

    public EntityDamageRank Damage { get; set; } = default!;

    public EntityTakeDamageRank TakeDamage { get; set; } = default!;

    public List<EntityFloor> Floors { get; set; } = default!;
}
