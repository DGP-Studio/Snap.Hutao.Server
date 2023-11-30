// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

public class UidAndItems
{
    public string Uid { get; set; } = default!;

    public List<SimpleGachaItem> Items { get; set; } = default!;
}