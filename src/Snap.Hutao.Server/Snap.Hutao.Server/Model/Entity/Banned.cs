// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 封禁的Uid集合
/// </summary>
[Table("banned")]
public class Banned
{
    /// <summary>
    /// 封禁的Uid
    /// </summary>
    [Key]
    [StringLength(9, MinimumLength = 9)]
    public string Uid { get; set; } = default!;
}
