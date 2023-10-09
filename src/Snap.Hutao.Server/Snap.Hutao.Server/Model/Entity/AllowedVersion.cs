// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 日志允许的Header
/// </summary>
[Table("allowed_versions")]
public class AllowedVersion
{
    /// <summary>
    /// 头
    /// </summary>
    [Key]
    public string Header { get; set; } = default!;
}