// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Expire;

public class TermExtendResult
{
    public TermExtendResult(TermExtendResultKind kind, DateTimeOffset expiredAt = default)
    {
        Kind = kind;
        ExpiredAt = expiredAt;
    }

    public TermExtendResultKind Kind { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}