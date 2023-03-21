// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 注册或登录摄入数据
/// </summary>
public class Passport
{
    /// <summary>
    /// 用户名称
    /// </summary>
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = default!;
}