// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.SpiralAbyss;

[Table("records")]
public class EntityRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    [StringLength(10, MinimumLength = 9)]
    public string Uid { get; set; } = null!;

    [StringLength(64)]
    public string Uploader { get; set; } = null!;

    public long UploadTime { get; set; }

    public EntitySpiralAbyss? SpiralAbyss { get; set; }

    public List<EntityAvatar> Avatars { get; set; } = null!;
}