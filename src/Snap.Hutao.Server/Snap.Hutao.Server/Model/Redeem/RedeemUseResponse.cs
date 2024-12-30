// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Redeem;

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemUseResponse
{
    public RedeemUseResponse(RedeemStatus status)
    {
        Status = status;
    }

    public RedeemUseResponse(RedeemStatus status, RedeemCodeTargetServiceType type, DateTimeOffset expireTime)
    {
        Status = status;
        Type = type;
        ExpireTime = expireTime;
    }

    public RedeemStatus Status { get; set; }

    public RedeemCodeTargetServiceType Type { get; set; }

    public DateTimeOffset ExpireTime { get; set; }
}