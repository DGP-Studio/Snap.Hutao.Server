// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 武器相关构筑
/// </summary>
public abstract class WeaponBuild
{
    /// <summary>
    /// 构造一个新的角色构筑信息
    /// </summary>
    [JsonConstructor]
    public WeaponBuild()
    {
    }

    /// <summary>
    /// 构造一个新的武器构筑信息
    /// </summary>
    /// <param name="weaponId">武器Id</param>
    public WeaponBuild(int weaponId)
    {
        WeaponId = weaponId;
    }

    /// <summary>
    /// 武器Id
    /// </summary>
    public int WeaponId { get; set; }
}