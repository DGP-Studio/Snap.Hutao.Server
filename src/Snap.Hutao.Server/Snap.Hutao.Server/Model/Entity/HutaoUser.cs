// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 胡桃用户
/// </summary>
public sealed class HutaoUser : IdentityUser<int>
{
    /// <summary>
    /// 祈愿记录过期时间
    /// </summary>
    public long GachaLogExpireAt { get; set; }
}