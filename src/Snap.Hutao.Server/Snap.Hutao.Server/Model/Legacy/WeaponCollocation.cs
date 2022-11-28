// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 武器搭配
/// </summary>
public class WeaponCollocation : WeaponBuild
{
    /// <summary>
    /// 构造一个新的角色搭配
    /// </summary>
    /// <param name="weaponId">武器Id</param>
    public WeaponCollocation(int weaponId)
        : base(weaponId)
    {
    }

    /// <summary>
    /// 角色
    /// </summary>
    public List<ItemRate<int, double>> Avatars { get; set; } = default!;
}