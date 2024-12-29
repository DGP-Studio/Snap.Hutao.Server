// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemGenerateRequest
{
    [JsonPropertyName("n")]
    public uint Count { get; set; }

    [JsonPropertyName("t")]
    public uint Type { get; set; }

    [JsonPropertyName("v")]
    public int Value { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; } = default!;

    [JsonIgnore]
    public string Creator { get; set; } = default!;
}