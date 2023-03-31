// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 商品详细
/// </summary>
public sealed class SkuDetail
{
    /// <summary>
    /// 商品Id
    /// </summary>
    [JsonPropertyName("sku_id")]
    public string SkuId { get; set; } = default!;

    /// <summary>
    /// 数量
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 相册编号
    /// </summary>
    [JsonPropertyName("album_id")]
    public string AlbumId { get; set; } = default!;

    /// <summary>
    /// 相片
    /// </summary>
    [JsonPropertyName("pic")]
    public string Pic { get; set; } = default!;
}