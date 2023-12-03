﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.SpiralAbyss;

[Table("take_damage_ranks")]
[Index(nameof(Value))]
public class EntityTakeDamageRank
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public long SpiralAbyssId { get; set; }

    [ForeignKey(nameof(SpiralAbyssId))]
    public EntitySpiralAbyss SpiralAbyss { get; set; } = default!;

    public int AvatarId { get; set; }

    public int Value { get; set; }

    [StringLength(9, MinimumLength = 9)]
    public string Uid { get; set; } = default!;
}