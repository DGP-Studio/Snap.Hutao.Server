// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Model.GachaLog;

/// <summary>
/// 卡池物品
/// </summary>
public class SimpleGachaItem
{
    /// <summary>
    /// 祈愿记录分类
    /// </summary>
    public GachaConfigType GachaType { get; set; }

    /// <summary>
    /// 祈愿记录查询分类
    /// 合并保底的卡池使用此属性
    /// 仅4种（不含400）
    /// </summary>
    public GachaConfigType QueryType { get; set; }

    /// <summary>
    /// 物品Id
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// 获取时间
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }
}