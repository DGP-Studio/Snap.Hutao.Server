// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Legacy;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 统计控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的统计控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="memoryCache">内存缓存</param>
    public StatisticsController(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 获取总览数据
    /// </summary>
    /// <returns>总览数据</returns>
    [HttpGet("Overview")]
    public IActionResult Overview()
    {
        return GetStatistics<Overview>(LegacyStatistics.Overview);
    }

    /// <summary>
    /// 获取上场率
    /// </summary>
    /// <returns>上场率</returns>
    [HttpGet("Avatar/AttendanceRate")]
    public IActionResult AttendanceRate()
    {
        return GetStatistics<AvatarAppearanceRank>(LegacyStatistics.AvatarAppearanceRank);
    }

    /// <summary>
    /// 获取使用率
    /// </summary>
    /// <returns>使用率</returns>
    [HttpGet("Avatar/UtilizationRate")]
    public IActionResult UtilizationRate()
    {
        return GetStatistics<AvatarUsageRank>(LegacyStatistics.AvatarUsageRank);
    }

    /// <summary>
    /// 获取使用率
    /// </summary>
    /// <returns>使用率</returns>
    [HttpGet("Avatar/HoldingRate")]
    public IActionResult HoldingRate()
    {
        return GetStatistics<AvatarConstellationInfo>(LegacyStatistics.AvatarConstellationInfo);
    }

    /// <summary>
    /// 获取角色/武器/圣遗物搭配
    /// </summary>
    /// <returns>角色/武器/圣遗物搭配</returns>
    [HttpGet("Avatar/AvatarCollocation")]
    public IActionResult AvatarCollocation()
    {
        return GetStatistics<AvatarCollocation>(LegacyStatistics.AvatarCollocation);
    }

    /// <summary>
    /// 获取队伍上场
    /// </summary>
    /// <returns>队伍上场</returns>
    [HttpGet("Team/Combination")]
    public IActionResult Combination()
    {
        return GetStatistics<TeamAppearance>(LegacyStatistics.TeamAppearance);
    }

    private IActionResult GetStatistics<T>(string name)
        where T : class
    {
        int scheduleId = StatisticsHelper.GetScheduleId();
        T? data = StatisticsHelper.FromCacheOrDb<T>(appDbContext, memoryCache, scheduleId, name);

        return Response<T>.Success("获取统计数据成功", data!);
    }
}