// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.GachaLog;

[Table("gacha_statistics")]
public class GachaStatistics
{
    public const string AvaterEventGachaDistribution = "AvaterEventGachaDistribution";
    public const string WeaponEventGachaDistribution = "WeaponEventGachaDistribution";
    public const string StandardGachaDistribution = "StandardGachaDistribution";
    public const string ChronicledGachaDistribution = "ChronicledGachaDistribution";
    public const string GachaEventStatistics = "GachaEventStatistics";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public string Name { get; set; } = default!;

    public string Data { get; set; } = default!;

    public static GachaStatistics CreateWithName(string name)
    {
        return new()
        {
            Name = name,
        };
    }
}
