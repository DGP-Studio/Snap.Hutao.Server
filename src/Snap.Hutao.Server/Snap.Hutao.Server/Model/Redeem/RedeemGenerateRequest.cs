// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Redeem;

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemGenerateRequest
{
    [JsonPropertyName("n")]
    public uint Count { get; set; }

    [JsonPropertyName("t")]
    public RedeemCodeType Type { get; set; }

    [JsonPropertyName("s")]
    public RedeemCodeTargetServiceType ServiceType { get; set; }

    [JsonPropertyName("v")]
    public int Value { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("expire")]
    public DateTimeOffset ExpireTime { get; set; }

    [JsonPropertyName("times")]
    public uint Times { get; set; }

    [JsonIgnore]
    public string Creator { get; set; } = default!;
}