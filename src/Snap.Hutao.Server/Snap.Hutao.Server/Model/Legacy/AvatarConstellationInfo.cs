// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class AvatarConstellationInfo : AvatarBuild
{
    public AvatarConstellationInfo(int avatarId)
        : base(avatarId)
    {
    }

    public double HoldingRate { get; set; }

    public List<ItemRate<int, double>> Constellations { get; set; } = default!;
}