﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 使用率
/// </summary>
public class AvatarUsageRank
{
    /// <summary>
    /// 楼层
    /// </summary>
    public int Floor { get; set; }

    /// <summary>
    /// 排行
    /// </summary>
    public List<ItemRate<int, double>> Ranks { get; set; } = default!;
}