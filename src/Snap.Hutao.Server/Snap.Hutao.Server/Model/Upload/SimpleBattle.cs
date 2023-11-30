// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleBattle
{
    public int Index { get; set; }

    public List<int> Avatars { get; set; } = default!;
}