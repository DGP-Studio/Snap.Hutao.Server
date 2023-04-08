// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 祈愿记录服务到期时间
    /// </summary>
    public DateTimeOffset GachaLogExpireAt { get; set; }
}