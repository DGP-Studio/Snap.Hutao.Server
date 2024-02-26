// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class Author
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}