// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class AvatarCollocation : AvatarBuild
{
    public AvatarCollocation(int avatarId)
        : base(avatarId)
    {
    }

    public List<ItemRate<int, double>> Avatars { get; set; } = default!;

    public List<ItemRate<int, double>> Weapons { get; set; } = default!;

    public List<ItemRate<string, double>> Reliquaries { get; set; } = default!;
}