// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.RoleCombat;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.RoleCombat;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.RoleCombat;
using System.Collections.Concurrent;

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

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(GroupName = "RoleCombat")]
public class RoleCombatController
{
    private static readonly ConcurrentDictionary<string, UploadToken> UploadingUids = new();

    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public RoleCombatController(IServiceProvider serviceProvider)
    {
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    }

    public IActionResult Upload([FromBody] SimpleRoleCombatRecord record)
    {
        if (memoryCache.TryGetValue(RoleCombatService.Working, out _))
        {
            return Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", ServerKeys.ServerRecordComputingStatistics);
        }

        if (record.ScheduleId != RoleCombatScheduleId.GetForNow())
        {
            return Response.Fail(ReturnCode.NotCurrentSchedule, "非当前剧演数据", ServerKeys.ServerRecordNotCurrentSchedule);
        }

        if (!record.Validate())
        {
            return Response.Fail(ReturnCode.InvalidUploadData, "无效的提交数据", ServerKeys.ServerRecordInvalidData);
        }

        if (UploadingUids.TryGetValue(record.Uid, out _) || !UploadingUids.TryAdd(record.Uid, default))
        {
            return Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理", ServerKeys.ServerRecordPreviousRequestNotCompleted)
        }


    }

    [HttpGet("Statistics")]
    public IActionResult GetStatistics([FromQuery(Name = "Last")] bool last = false)
    {
        int scheduleId = RoleCombatScheduleId.GetForNow();
        if (last)
        {
            scheduleId--;
        }

        string key = $"RoleCombatStatistics:{scheduleId}";
        if (memoryCache.TryGetValue(key, out RoleCombatStatisticsItem? data))
        {
            return Response<RoleCombatStatisticsItem>.Success("获取剧演统计数据成功", data!);
        }

        RoleCombatStatistics? statistics = appDbContext.RoleCombatStatistics
            .SingleOrDefault(s => s.ScheduleId == scheduleId);

        if (statistics is null)
        {
            return Response<RoleCombatStatisticsItem>.Success("获取剧演统计数据成功", default!);
        }

        RoleCombatStatisticsItem? typedData = JsonSerializer.Deserialize<RoleCombatStatisticsItem>(statistics.Data);
        memoryCache.Set(key, typedData, TimeSpan.FromDays(1));

        return Response<RoleCombatStatisticsItem>.Success("获取深渊统计数据成功", typedData!);
    }

    private readonly struct UploadToken;
}