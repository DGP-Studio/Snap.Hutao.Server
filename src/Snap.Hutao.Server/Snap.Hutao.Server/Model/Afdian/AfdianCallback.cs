// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 爱发电回调
/// </summary>
public sealed class AfdianCallback
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