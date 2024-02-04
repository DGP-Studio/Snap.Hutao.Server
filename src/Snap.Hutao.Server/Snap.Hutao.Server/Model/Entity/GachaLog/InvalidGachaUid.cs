// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.GachaLog;

[Table("invalid_gacha_uids")]
public class InvalidGachaUid
{
    [Key]
    [StringLength(10, MinimumLength = 9)]
    public string Uid { get; set; } = default!;
}