// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public abstract class AvatarBuild
{
    public AvatarBuild(int avatarId)
    {
        AvatarId = avatarId;
    }

    public int AvatarId { get; set; }
}
