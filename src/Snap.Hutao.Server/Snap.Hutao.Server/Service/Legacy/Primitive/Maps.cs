// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 映射相关
/// </summary>
public static class Maps
{
    /// <summary>
    /// 构造一个新的为命座统计准备的映射
    /// </summary>
    /// <param name="avatarId">Not used</param>
    /// <returns>映射</returns>
    public static Map<Constellation, int> ForConstellation(AvatarId avatarId)
    {
        return new()
        {
            [0] = 0,
            [1] = 0,
            [2] = 0,
            [3] = 0,
            [4] = 0,
            [5] = 0,
            [6] = 0,
        };
    }
}