// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Redeem;

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemGenerateRequest
{
    public uint Count { get; set; }

    public RedeemCodeType Type { get; set; }

    public RedeemCodeTargetServiceType ServiceType { get; set; }

    public int Value { get; set; }

    public string Description { get; set; } = default!;

    public DateTimeOffset ExpireTime { get; set; }

    public uint Times { get; set; }

    [JsonIgnore]
    public string Creator { get; set; } = default!;
}