// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class Release
{
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; } = default!;

    [JsonPropertyName("body")]
    public string Body { get; set; } = default!;
}