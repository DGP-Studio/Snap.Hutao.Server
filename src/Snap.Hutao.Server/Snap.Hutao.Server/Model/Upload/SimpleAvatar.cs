// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleAvatar
{
    public int AvatarId { get; set; }

    public int WeaponId { get; set; }

    public List<int> ReliquarySetIds { get; set; } = default!;

    public int ActivedConstellationNumber { get; set; }
}