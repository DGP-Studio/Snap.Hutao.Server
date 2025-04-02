// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.StaticResource;

public sealed class ImageToken
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = default!;

    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    [JsonPropertyName("expire")]
    public long Expire { get; set; }
}