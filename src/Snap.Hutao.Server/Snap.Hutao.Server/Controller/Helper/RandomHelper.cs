// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Controller.Helper;

/// <summary>
/// 随机帮助类
/// </summary>
public static class RandomHelper
{
    private const string RandomRange = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    /// <summary>
    /// 获取随机字符串
    /// </summary>
    /// <param name="count">长度</param>
    /// <returns>随机字符串</returns>
    public static string GetRandomStringWithChars(int count)
    {
        StringBuilder sb = new(count);

        for (int i = 0; i < count; i++)
        {
            int pos = Random.Shared.Next(0, RandomRange.Length);
            sb.Append(RandomRange[pos]);
        }

        return sb.ToString();
    }
}