// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 胡桃上传的日志
/// </summary>
[Table("hutao_logs")]
public class HutaoLog
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Info { get; set; } = default!;

    /// <summary>
    /// 个数
    /// </summary>
    public int Count { get; set; } = default!;

    /// <summary>
    /// 是否已经解决
    /// </summary>
    public bool Resolved { get; set; }

    /// <summary>
    /// 发生版本
    /// </summary>
    public string Version { get; set; } = default!;
}