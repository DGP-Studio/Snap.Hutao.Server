// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleLevel
{
    public int Index { get; set; }

    public int Star { get; set; }

    public List<SimpleBattle> Battles { get; set; } = default!;
}
