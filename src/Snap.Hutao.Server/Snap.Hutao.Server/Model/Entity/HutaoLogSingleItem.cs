// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 单个日志信息
/// </summary>
[Table("hutao_log_items")]
public class HutaoLogSingleItem
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    /// <summary>
    /// 外键
    /// </summary>
    public long LogId { get; set; }

    /// <summary>
    /// 外键对象
    /// </summary>
    [ForeignKey(nameof(LogId))]
    public HutaoLog Log { get; set; } = default!;

    /// <summary>
    /// 设备Id
    /// </summary>
    [MaxLength(32)]
    public string DeviceId { get; set; } = default!;

    /// <summary>
    /// 崩溃时间
    /// </summary>
    public long Time { get; set; }
}