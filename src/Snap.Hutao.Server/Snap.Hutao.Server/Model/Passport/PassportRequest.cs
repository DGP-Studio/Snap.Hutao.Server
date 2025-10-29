// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public sealed class PassportRequest
{
    /// <summary>
    /// 使用服务端下发的 RSA 公钥加密并 Base64 编码后的登录邮箱。
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 使用 RSA 加密并 Base64 编码后的新邮箱账号（用于账号迁移）。
    /// </summary>
    public string NewUserName { get; set; } = default!;

    /// <summary>
    /// 使用 RSA 加密并 Base64 编码后的密码字段。
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// 邮件中收到的 6 位数字验证码。
    /// </summary>
    public string VerifyCode { get; set; } = default!;

    /// <summary>
    /// 新邮箱验证码，配合账号迁移使用。
    /// </summary>
    public string NewVerifyCode { get; set; } = default!;

    /// <summary>
    /// 指示请求用于重置密码。
    /// </summary>
    public bool IsResetPassword { get; set; }

    /// <summary>
    /// 指示请求用于校验旧邮箱。
    /// </summary>
    public bool IsResetUserName { get; set; }

    /// <summary>
    /// 指示请求用于校验新邮箱。
    /// </summary>
    public bool IsResetUserNameNew { get; set; }

    /// <summary>
    /// 指示请求用于注销账号。
    /// </summary>
    public bool IsCancelRegistration { get; set; }
}