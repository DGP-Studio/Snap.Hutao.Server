// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 加密的账密
/// </summary>
public class EncryptedPassport : EncryptedUserName
{
    /// <summary>
    /// RSA加密 密码
    /// </summary>
    public string Password { get; set; } = default!;
}