// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.GachaLog;

namespace Snap.Hutao.Server.Model.Metadata;

public static class GachaEventInfoExtension
{
    public static IEnumerable<int> GetUpOrangeItems(this GachaEventInfo info)
    {
        return typeof(GachaEventInfo)
            .GetProperties()
            .Where(p => p.Name.StartsWith("UpOrangeItem"))
            .Select(p => (uint)p.GetValue(info)!)
            .Where(v => v is not 0U)
            .Select(v => (int)v);
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
