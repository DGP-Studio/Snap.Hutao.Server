// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 加密的账号
/// </summary>
public class EncryptedUserName
{
    /// <summary>
    /// RSA加密 用户名称
    /// </summary>
    public string UserName { get; set; } = default!;
}