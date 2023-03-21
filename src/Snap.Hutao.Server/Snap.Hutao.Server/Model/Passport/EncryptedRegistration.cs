// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 加密的账号与验证码
/// </summary>
public class EncryptedRegistration : EncryptedPassport
{
    /// <summary>
    /// RSA加密 验证码
    /// </summary>
    public string VerifyCode { get; set; } = default!;
}