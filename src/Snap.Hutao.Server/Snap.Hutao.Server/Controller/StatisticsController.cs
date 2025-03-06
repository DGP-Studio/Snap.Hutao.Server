// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "SpiralAbyss")]
public class StatisticsController : ControllerBase
{
    private readonly SpiralAbyssStatisticsService spiralAbyssStatisticsService;

    public StatisticsController(SpiralAbyssStatisticsService spiralAbyssStatisticsService)
    {
        this.spiralAbyssStatisticsService = spiralAbyssStatisticsService;
    }

    [HttpGet("Overview")]
    public IActionResult Overview([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<Overview>(LegacyStatistics.Overview, last);
    }

    [HttpGet("Avatar/AttendanceRate")]
    public IActionResult AttendanceRate([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<AvatarAppearanceRank>>(LegacyStatistics.AvatarAppearanceRank, last);
    }

    [HttpGet("Avatar/UtilizationRate")]
    public IActionResult UtilizationRate([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<AvatarUsageRank>>(LegacyStatistics.AvatarUsageRank, last);
    }

    [HttpGet("Avatar/HoldingRate")]
    public IActionResult HoldingRate([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<AvatarConstellationInfo>>(LegacyStatistics.AvatarConstellationInfo, last);
    }

    [HttpGet("Avatar/AvatarCollocation")]
    public IActionResult AvatarCollocation([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<AvatarCollocation>>(LegacyStatistics.AvatarCollocation, last);
    }

    [HttpGet("Weapon/WeaponCollocation")]
    public IActionResult WeaponCollocation([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<WeaponCollocation>>(LegacyStatistics.WeaponCollocation, last);
    }

    [HttpGet("Team/Combination")]
    public IActionResult Combination([FromQuery(Name = "Last")] bool last = false)
    {
        return GetStatistics<List<TeamAppearance>>(LegacyStatistics.TeamAppearance, last);
    }

    private IActionResult GetStatistics<T>(string name, bool last)
        where T : class
    {
        int scheduleId = SpiralAbyssScheduleId.GetForNow();
        if (last)
        {
            scheduleId--;
        }

        return Response<T>.Success("获取深渊统计数据成功", spiralAbyssStatisticsService.GetStatistics<T>(name, scheduleId)!);
    }
}