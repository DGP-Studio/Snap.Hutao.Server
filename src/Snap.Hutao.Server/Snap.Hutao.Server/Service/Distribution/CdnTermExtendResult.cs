// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Distribution;

public class CdnTermExtendResult
{
    public CdnTermExtendResult(CdnTermExtendResultKind kind, DateTimeOffset expiredAt = default)
    {
        Kind = kind;
        ExpiredAt = expiredAt;
    }

    public CdnTermExtendResultKind Kind { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}