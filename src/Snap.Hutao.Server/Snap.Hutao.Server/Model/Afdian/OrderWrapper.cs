// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 订单包装器
/// </summary>
public sealed class OrderWrapper
{
    /// <summary>
    /// "Order"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    /// <summary>
    /// 订单
    /// </summary>
    [JsonPropertyName("order")]
    public Order Order { get; set; } = default!;
}