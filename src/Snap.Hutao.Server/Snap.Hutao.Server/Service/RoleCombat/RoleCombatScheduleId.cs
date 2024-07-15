// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.RoleCombat;

public static class RoleCombatScheduleId
{
    private static readonly TimeSpan Utc8 = new(8, 0, 0);

    public static int GetForNow()
    {
        return GetForDateTimeOffset(DateTimeOffset.UtcNow);
    }

    public static int GetForDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        // Force time in UTC+08
        dateTimeOffset = dateTimeOffset.ToOffset(Utc8);

        ((int year, int mouth, int day), (int hour, _), _) = dateTimeOffset;

        // 2024-07-01 04:00:00 为第 3 期
        int periodNum = ((year - 2024) * 12) + (mouth - 6) + 2;

        // 上个月：1 日 00:00-04:00
        if (day is 1 && hour < 4)
        {
            periodNum--;
        }

        return periodNum;
    }
}