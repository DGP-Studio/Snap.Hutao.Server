// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

public sealed class GachaEntry
{
    public string Uid { get; set; } = default!;

    public bool Excluded { get; set; }

    public int ItemCount { get; set; }
}