// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 祈愿物品
/// </summary>
[Table("gacha_items")]
[PrimaryKey(nameof(Uid), nameof(Id))]
public sealed class EntityGachaItem
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 可以参与统计
    /// </summary>
    public bool IsTrusted { get; set; }

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
}