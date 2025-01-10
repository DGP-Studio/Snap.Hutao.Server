// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Model.Entity.Redeem;

[Table("redeem_code_use_items")]
public sealed class RedeemCodeUseItem
{
    public uint RedeemCodeId { get; set; }

    [ForeignKey(nameof(RedeemCodeId))]
    public RedeemCode RedeemCode { get; set; } = default!;

    [Key]
    public uint Id { get; set; }

    public int UsedBy { get; set; } = default!;

    [ForeignKey(nameof(UsedBy))]
    public HutaoUser User { get; set; } = default!;

    public DateTimeOffset UseTime { get; set; }
}