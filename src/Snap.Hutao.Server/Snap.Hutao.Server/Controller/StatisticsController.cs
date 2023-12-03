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
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(GroupName = "SpiralAbyss")]
public class StatisticsController : ControllerBase
{
    private readonly SpiralAbyssStatisticsService spiralAbyssStatisticsService;

    public StatisticsController(SpiralAbyssStatisticsService spiralAbyssStatisticsService)
    {
        this.spiralAbyssStatisticsService = spiralAbyssStatisticsService;
    }

    [HttpGet("Overview")]
    public IActionResult Overview()
    {
        return GetStatistics<Overview>(LegacyStatistics.Overview);
    }

    [HttpGet("Avatar/AttendanceRate")]
    public IActionResult AttendanceRate()
    {
        return GetStatistics<List<AvatarAppearanceRank>>(LegacyStatistics.AvatarAppearanceRank);
    }

    [HttpGet("Avatar/UtilizationRate")]
    public IActionResult UtilizationRate()
    {
        return GetStatistics<List<AvatarUsageRank>>(LegacyStatistics.AvatarUsageRank);
    }

    [HttpGet("Avatar/HoldingRate")]
    public IActionResult HoldingRate()
    {
        return GetStatistics<List<AvatarConstellationInfo>>(LegacyStatistics.AvatarConstellationInfo);
    }

    [HttpGet("Avatar/AvatarCollocation")]
    public IActionResult AvatarCollocation()
    {
        return GetStatistics<List<AvatarCollocation>>(LegacyStatistics.AvatarCollocation);
    }

    [HttpGet("Weapon/WeaponCollocation")]
    public IActionResult WeaponCollocation()
    {
        return GetStatistics<List<WeaponCollocation>>(LegacyStatistics.WeaponCollocation);
    }

    [HttpGet("Team/Combination")]
    public IActionResult Combination()
    {
        return GetStatistics<List<TeamAppearance>>(LegacyStatistics.TeamAppearance);
    }

    private IActionResult GetStatistics<T>(string name)
        where T : class
    {
        return Response<T>.Success("获取深渊统计数据成功", spiralAbyssStatisticsService.GetStatistics<T>(name)!);
    }
}