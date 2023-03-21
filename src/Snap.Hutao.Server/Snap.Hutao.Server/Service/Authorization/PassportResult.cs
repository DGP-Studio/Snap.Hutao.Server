// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 通行证结果
/// </summary>
public class PassportResult
{
    /// <summary>
    /// 构造一个新的通行证结果
    /// </summary>
    /// <param name="success">是否成功</param>
    /// <param name="message">消息</param>
    /// <param name="token">令牌</param>
    public PassportResult(bool success, string message, string? token = null)
    {
        Success = success;
        Message = message;
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
}