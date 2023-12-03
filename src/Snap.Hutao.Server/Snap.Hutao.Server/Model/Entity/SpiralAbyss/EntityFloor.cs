// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Model.Entity.SpiralAbyss;

[Table("spiral_abysses_floors")]
public class EntityFloor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public long SpiralAbyssId { get; set; }

    [ForeignKey(nameof(SpiralAbyssId))]
    public EntitySpiralAbyss SpiralAbyss { get; set; } = default!;

    public int Index { get; set; }

    public int Star { get; set; }

    // Will be json string in database
    public List<SimpleLevel> Levels { get; set; } = default!;
}