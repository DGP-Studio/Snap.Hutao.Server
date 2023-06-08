// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

[Table("gacha_statistics")]
public class GachaStatistics
{
    public const string AvaterEventGachaDistribution = "AvaterEventGachaDistribution";
    public const string WeaponEventGachaDistribution = "WeaponEventGachaDistribution";
    public const string StandardGachaDistribution = "StandardGachaDistribution";
    public const string GachaEventStatistics = "GachaEventStatistics";

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    /// <summary>
    /// 数据名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 数据
    /// </summary>
    public string Data { get; set; } = default!;

    public static GachaStatistics Create(string name)
    {
        return new()
        {
            Name = name,
        };
    }
}
