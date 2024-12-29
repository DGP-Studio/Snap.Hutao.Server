// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemUseResponse
{
    public RedeemUseResponse(RedeemStatus status)
    {
        Status = status;
    }

    public RedeemUseResponse(RedeemStatus status, uint type, DateTimeOffset expireTime)
    {
        Status = status;
        Type = type;
        ExpireTime = expireTime;
    }

    public RedeemStatus Status { get; set; }

    public uint Type { get; set; }

    public DateTimeOffset ExpireTime { get; set; }
}