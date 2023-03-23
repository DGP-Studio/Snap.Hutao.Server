// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 验证请求
/// </summary>
public sealed class EncryptedVerification : EncryptedUserName
{
    /// <summary>
    /// 是否为重置密码请求
    /// </summary>
    public bool IsResetPassword { get; set; }
}