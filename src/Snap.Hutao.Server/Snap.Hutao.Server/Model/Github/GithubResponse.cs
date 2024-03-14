// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class GithubResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
}