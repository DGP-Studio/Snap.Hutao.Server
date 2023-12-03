// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.GachaLog;

public sealed class GachaLogSaveResult
{
    public GachaLogSaveResult(GachaLogSaveResultKind kind, int save = 0)
    {
        Kind = kind;
        SaveCount = save;
    }

    public GachaLogSaveResultKind Kind { get; }

    public int SaveCount { get; }
}