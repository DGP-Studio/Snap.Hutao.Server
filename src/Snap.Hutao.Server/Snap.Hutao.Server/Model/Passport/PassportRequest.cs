// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 通行证请求
/// </summary>
public sealed class PassportRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// 验证码
    /// </summary>
    public string VerifyCode { get; set; } = default!;

    /// <summary>
    /// 是否为重置密码请求
    /// </summary>
    public bool IsResetPassword { get; set; }
}
