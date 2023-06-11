// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

[Table("invalid_gacha_uids")]
public class InvalidGachaUid
{
    /// <summary>
    /// 无效祈愿记录的Uid
    /// </summary>
    [Key]
    [StringLength(9, MinimumLength = 9)]
    public string Uid { get; set; } = default!;
}