// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public abstract class WeaponBuild
{
    public WeaponBuild(int weaponId)
    {
        WeaponId = weaponId;
    }

    public int WeaponId { get; set; }
}