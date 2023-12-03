// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class AvatarAppearanceRank
{
    public int Floor { get; set; }

    public List<ItemRate<int, double>> Ranks { get; set; } = default!;
}