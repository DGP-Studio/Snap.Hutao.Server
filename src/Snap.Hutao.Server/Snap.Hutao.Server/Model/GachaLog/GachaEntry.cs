// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

/// <summary>
/// 祈愿Entry
/// </summary>
public sealed class GachaEntry
{
    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// 物品个数
    /// </summary>
    public int ItemCount { get; set; }
}