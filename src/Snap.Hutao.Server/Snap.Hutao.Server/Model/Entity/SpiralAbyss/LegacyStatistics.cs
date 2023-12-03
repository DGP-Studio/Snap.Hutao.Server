// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.SpiralAbyss;

[Table("spiral_abysses_statistics")]
public class LegacyStatistics
{
    public const string Overview = "Overview";
    public const string AvatarUsageRank = "AvatarUsageRank";
    public const string AvatarAppearanceRank = "AvatarAppearanceRank";
    public const string AvatarConstellationInfo = "AvatarConstellationInfo";
    public const string AvatarCollocation = "AvatarCollocation";
    public const string WeaponCollocation = "WeaponCollocation";
    public const string TeamAppearance = "TeamAppearance";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public int ScheduleId { get; set; }

    public string Name { get; set; } = default!;

    public string Data { get; set; } = default!;

    public static LegacyStatistics CreateWithNameAndScheduleId(string name, int scheduleId)
    {
        return new()
        {
            Name = name,
            ScheduleId = scheduleId,
        };
    }
}