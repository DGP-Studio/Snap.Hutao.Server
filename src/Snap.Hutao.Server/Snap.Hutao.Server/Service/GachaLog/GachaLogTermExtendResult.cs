// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.GachaLog;

public class GachaLogTermExtendResult
{
    public GachaLogTermExtendResult(GachaLogTermExtendResultKind kind, DateTimeOffset expiredAt = default)
    {
        Kind = kind;
        ExpiredAt = expiredAt;
    }

    public GachaLogTermExtendResultKind Kind { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}