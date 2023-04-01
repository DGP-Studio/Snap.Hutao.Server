// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 列表包装器
/// </summary>
/// <typeparam name="T">元素类型</typeparam>
public class ListWrapper<T>
{
    /// <summary>
    /// 列表
    /// </summary>
    [JsonPropertyName("list")]
    public List<T> List { get; set; } = default!;
}