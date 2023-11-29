// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy;

public static class SpiralAbyssScheduleId
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

        (DateOnly date, TimeOnly time, _) = dateTimeOffset;
        (int year, int mouth, int day) = date;
        (int hour, _) = time;

        // 2020-07-01 04:00:00 为第 1 期
        int periodNum = (((year - 2020) * 12) + (mouth - 6)) * 2;

        // 上半月：1-15 日, 以及 16 日 00:00-04:00
        if (day < 16 || (day == 16 && hour < 4))
        {
            periodNum--;
        }

        // 上个月：1 日 00:00-04:00
        if (day is 1 && hour < 4)
        {
            periodNum--;
        }

        return periodNum - 12;
    }
}