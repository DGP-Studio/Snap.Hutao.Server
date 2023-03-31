// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 爱发电请求
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class AfdianRequest<T>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    [JsonPropertyName("ec")]
    public int ErrorCode { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [JsonPropertyName("em")]
    public string ErrorMessage { get; set; } = default!;

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;
}