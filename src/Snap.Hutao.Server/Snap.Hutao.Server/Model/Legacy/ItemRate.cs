// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class ItemRate<TItem, TRate>
{
    public ItemRate(TItem item, TRate rate)
    {
        Item = item;
        Rate = rate;
    }

    public TItem Item { get; set; }

    public TRate Rate { get; set; }
}