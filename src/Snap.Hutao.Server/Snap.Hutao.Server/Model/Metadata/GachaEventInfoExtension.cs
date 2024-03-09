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

        if (info.UpOrangeItem3 != 0U)
        {
            yield return (int)info.UpOrangeItem3;
        }

        if (info.UpOrangeItem4 != 0U)
        {
            yield return (int)info.UpOrangeItem4;
        }

        if (info.UpOrangeItem5 != 0U)
        {
            yield return (int)info.UpOrangeItem5;
        }

        if (info.UpOrangeItem6 != 0U)
        {
            yield return (int)info.UpOrangeItem6;
        }

        if (info.UpOrangeItem7 != 0U)
        {
            yield return (int)info.UpOrangeItem7;
        }

        if (info.UpOrangeItem8 != 0U)
        {
            yield return (int)info.UpOrangeItem8;
        }

        if (info.UpOrangeItem9 != 0U)
        {
            yield return (int)info.UpOrangeItem9;
        }

        if (info.UpOrangeItem10 != 0U)
        {
            yield return (int)info.UpOrangeItem10;
        }

        if (info.UpOrangeItem11 != 0U)
        {
            yield return (int)info.UpOrangeItem11;
        }

        if (info.UpOrangeItem12 != 0U)
        {
            yield return (int)info.UpOrangeItem12;
        }

        if (info.UpOrangeItem13 != 0U)
        {
            yield return (int)info.UpOrangeItem13;
        }

        if (info.UpOrangeItem14 != 0U)
        {
            yield return (int)info.UpOrangeItem14;
        }

        if (info.UpOrangeItem15 != 0U)
        {
            yield return (int)info.UpOrangeItem15;
        }

        if (info.UpOrangeItem16 != 0U)
        {
            yield return (int)info.UpOrangeItem16;
        }

        if (info.UpOrangeItem17 != 0U)
        {
            yield return (int)info.UpOrangeItem17;
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
