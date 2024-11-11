// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.RoleCombat;

[Table("role_combat_records")]
public sealed class RoleCombatRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    [StringLength(10, MinimumLength = 9)]
    public string Uid { get; set; } = null!;

    [StringLength(64)]
    public string Uploader { get; set; } = null!;

    public long UploadTime { get; set; }

    public List<RoleCombatAvatar> Avatars { get; set; } = null!;
}