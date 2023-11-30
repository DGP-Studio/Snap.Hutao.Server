// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;

namespace Snap.Hutao.Server.Model.GachaLog;

public class SimpleGachaItem
{
    public GachaConfigType GachaType { get; set; }

    public GachaConfigType QueryType { get; set; }

    public int ItemId { get; set; }

    public DateTimeOffset Time { get; set; }

    public long Id { get; set; }
}