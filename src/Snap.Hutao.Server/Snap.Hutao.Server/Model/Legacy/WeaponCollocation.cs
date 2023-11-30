// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class WeaponCollocation : WeaponBuild
{
    public WeaponCollocation(int weaponId)
        : base(weaponId)
    {
    }

    public List<ItemRate<int, double>> Avatars { get; set; } = default!;
}