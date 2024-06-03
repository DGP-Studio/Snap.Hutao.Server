// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy;

public static class SpiralAbyssScheduleId
{
    private static readonly TimeSpan Utc8 = new(8, 0, 0);
    private static readonly DateTimeOffset AcrobaticsBattleIntroducedTime = new(2024, 7, 1, 4, 0, 0, Utc8);

    public static int GetForNow()
    {
        return GetForDateTimeOffset(DateTimeOffset.UtcNow);
    }

    public static int GetForDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        // Force time in UTC+08
        dateTimeOffset = dateTimeOffset.ToOffset(Utc8);

        ((int year, int mouth, int day), (int hour, _), _) = dateTimeOffset;

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

        if (dateTimeOffset >= AcrobaticsBattleIntroducedTime)
        {
            // 当超过 96 期时，每一个月一期
            periodNum = (4 * 12 * 2) + ((periodNum - (4 * 12 * 2)) / 2);
        }

        return periodNum;
    }
}