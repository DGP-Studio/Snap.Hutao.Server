// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

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

    /// <summary>
    /// 是否为获取了许可的开发者
    /// </summary>
    public bool IsLicensedDeveloper { get; set; }

    /// <summary>
    /// 是否为管理员
    /// </summary>
    public bool IsMaintainer { get; set; }
}