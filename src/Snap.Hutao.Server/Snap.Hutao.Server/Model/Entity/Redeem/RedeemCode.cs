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

    public RedeemCodeType Type { get; set; }

    public RedeemCodeTargetServiceType ServiceType { get; set; }

    public int Value { get; set; }

    public string Description { get; set; } = default!;

    public string Creator { get; set; } = default!;

    public DateTimeOffset CreateTime { get; set; }

    public List<RedeemCodeUseItem> UseItems { get; set; } = default!;

    #region Time-limited

    public DateTimeOffset ExpireTime { get; set; }

    #endregion

    #region Times-limited

    public uint TimesAllowed { get; set; }

    #endregion
}