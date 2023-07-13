// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 爱发电回调
/// </summary>
public sealed class AfdianResponse
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
}

/// <summary>
/// 爱发电请求
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
[SuppressMessage("", "SA1402")]
public class AfdianResponse<T>
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