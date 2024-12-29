// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Redeem;

[Table("redeem_codes")]
public sealed class RedeemCode
{
    [Key]
    public uint Id { get; set; }

    [StringLength(24)]
    public string Code { get; set; } = default!;

    // 1: Gacha Log, 2: CDN
    public uint Type { get; set; }

    public int Value { get; set; }

    public bool IsUsed { get; set; }

    public string Description { get; set; } = default!;

    public string Creator { get; set; } = default!;

    public DateTimeOffset CreateTime { get; set; }

    public string UseBy { get; set; } = default!;

    public DateTimeOffset UseTime { get; set; }
}