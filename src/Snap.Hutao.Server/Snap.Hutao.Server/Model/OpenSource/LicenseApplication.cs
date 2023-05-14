// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.OpenSource;

/// <summary>
/// 许可申请
/// </summary>
public class LicenseApplication
{
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 项目地址
    /// </summary>
    public string ProjectUrl { get; set; } = default!;

    /// <summary>
    /// reCAPTCHA token
    /// </summary>
    public string Token { get; set; } = default!;
}
