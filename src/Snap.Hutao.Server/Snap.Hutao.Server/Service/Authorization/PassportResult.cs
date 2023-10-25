// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 通行证结果
/// </summary>
public class PassportResult
{
    public PassportResult(bool success, string message, string? key, string? token = null)
    {
        Success = success;
        Message = message;
        LocalizationKey = key;
        Token = token;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Token))]
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 令牌
    /// </summary>
    public string? Token { get; set; }

    public string? LocalizationKey { get; set; }
}