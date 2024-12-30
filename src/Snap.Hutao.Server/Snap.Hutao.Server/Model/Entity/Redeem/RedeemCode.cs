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

    #region One-time

    public bool IsUsed { get; set; }

    public string UseBy { get; set; } = default!;

    public DateTimeOffset UseTime { get; set; }

    #endregion

    #region Time-limited

    public DateTimeOffset ExpireTime { get; set; }

    public uint UseCount { get; set; }

    #endregion

    #region Times-limited

    public uint TimesRemain { get; set; }

    #endregion
}