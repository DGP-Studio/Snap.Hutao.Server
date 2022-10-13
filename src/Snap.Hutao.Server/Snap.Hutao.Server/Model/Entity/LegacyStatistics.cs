// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 统计信息
/// </summary>
[Table("spiral_abysses_statistics")]
public class LegacyStatistics
{
    /// <summary>
    /// 统计数据
    /// </summary>
    public const string Overview = "Overview";

    /// <summary>
    /// 角色使用率
    /// </summary>
    public const string AvatarUsageRank = "AvatarUsageRank";

    /// <summary>
    /// 角色出场率
    /// </summary>
    public const string AvatarAppearanceRank = "AvatarAppearanceRank";

    /// <summary>
    /// 角色命座持有率信息
    /// </summary>
    public const string AvatarConstellationInfo = "AvatarConstellationInfo";

    /// <summary>
    /// 角色搭配信息
    /// </summary>
    public const string AvatarCollocation = "AvatarCollocation";

    /// <summary>
    /// 队伍出场
    /// </summary>
    public const string TeamAppearance = "TeamAppearance";

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrimaryId { get; set; }

    /// <summary>
    /// 深渊计划Id
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// 数据名称
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// 数据
    /// </summary>
    public string Data { get; set; } = default!;

    /// <summary>
    /// 构造一个新的统计
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="scheduleId">计划Id</param>
    /// <returns>新的统计</returns>
    public static LegacyStatistics Create(string name, int scheduleId)
    {
        return new()
        {
            Name = name,
            ScheduleId = scheduleId,
        };
    }
}
