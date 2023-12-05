// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.GachaLog;

namespace Snap.Hutao.Server.Model.Metadata;

public static class GachaEventInfoExtension
{
    public static IEnumerable<int> GetUpOrangeItems(this GachaEventInfo info)
    {
        if (info.UpOrangeItem1 != 0U)
        {
            yield return (int)info.UpOrangeItem1;
        }

        if (info.UpOrangeItem2 != 0U)
        {
            yield return (int)info.UpOrangeItem2;
        }
    }

    public static bool ItemInThisEvent(this GachaEventInfo info, EntityGachaItem item)
    {
        if (item.GachaType == info.Type && item.Time >= info.From && item.Time <= info.To)
        {
            return true;
        }

        return false;
    }
}
