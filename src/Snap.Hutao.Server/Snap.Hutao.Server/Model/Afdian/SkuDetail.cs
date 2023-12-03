// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Afdian;

public sealed class SkuDetail
{
    [JsonPropertyName("sku_id")]
    public string SkuId { get; set; } = default!;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("album_id")]
    public string AlbumId { get; set; } = default!;

    [JsonPropertyName("pic")]
    public string Pic { get; set; } = default!;

    [JsonPropertyName("stock")]
    public string Stock { get; set; } = default!;

    [JsonPropertyName("post_id")]
    public string PostId { get; set; } = default!;
}