// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

internal sealed class GachaDistribution
{
    public long TotalValidPulls { get; set; }

    public List<PullCount> Distribution { get; set; } = default!;
}