// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleFloor
{
    public int Index { get; set; }

    public int Star { get; set; }

    public List<SimpleLevel> Levels { get; set; } = default!;
}
