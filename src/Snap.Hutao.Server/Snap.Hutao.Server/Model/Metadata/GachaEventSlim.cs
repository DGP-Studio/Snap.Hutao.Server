// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Metadata;

internal sealed class GachaEventSlim
{
    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }

    public GachaConfigType Type { get; set; }
}