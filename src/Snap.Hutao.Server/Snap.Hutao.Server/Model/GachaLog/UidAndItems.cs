// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

/// <summary>
/// 卡池列表
/// </summary>
public class UidAndItems
{
    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// 列表
    /// </summary>
    public List<SimpleGachaItem> Items { get; set; } = default!;
}