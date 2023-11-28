// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Metadata;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.GachaLog;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(GroupName = "GachaLog")]
public class GachaLogController : ControllerBase
{
    private readonly GachaLogService gachaLogService;

    public GachaLogController(GachaLogService gachaLogService)
    {
        this.gachaLogService = gachaLogService;
    }

    [HttpGet("Statistics/CurrentEventStatistics")]
    public IActionResult CurrentEventStatistics()
    {
        return GetGachaLogStatistics<GachaEventStatistics>(GachaStatistics.GachaEventStatistics);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("Statistics/Distribution/AvatarEvent")]
    public IActionResult AvatarEventDistribution()
    {
        return GetGachaLogStatistics<GachaDistribution>(GachaStatistics.AvaterEventGachaDistribution);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("Statistics/Distribution/WeaponEvent")]
    public IActionResult WeaponEventDistribution()
    {
        return GetGachaLogStatistics<GachaDistribution>(GachaStatistics.WeaponEventGachaDistribution);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("Statistics/Distribution/Standard")]
    public IActionResult StandardDistribution()
    {
        return GetGachaLogStatistics<GachaDistribution>(GachaStatistics.StandardGachaDistribution);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("Entries")]
    public async Task<IActionResult> GetGachaEntriesAsync()
    {
        List<GachaEntry> entries = await gachaLogService.GetGachaEntriesForUserAsync(this.GetUserId()).ConfigureAwait(false);
        return Response<List<GachaEntry>>.Success("获取 Entry 成功", entries);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("EndIds")]
    public async Task<IActionResult> GetEndIdsAsync([FromQuery(Name = "Uid")] string uid)
    {
        EndIds endIds = await gachaLogService.GetNewestEndIdsAsync(this.GetUserId(), uid).ConfigureAwait(false);
        return Response<EndIds>.Success("获取最新的 EndIds 成功", endIds);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpPost("Retrieve")]
    public async Task<IActionResult> RetrieveAsync([FromBody] UidAndEndIds uidAndEndIds)
    {
        int userId = this.GetUserId();
        List<SimpleGachaItem> gachaItems = await gachaLogService.GetGachaItemsEarlyThanEndIdsAsync(userId, uidAndEndIds).ConfigureAwait(false);
        return Response<List<SimpleGachaItem>>.Success(userId.ToString(), gachaItems);
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] UidAndItems uidAndItems)
    {
        int userId = this.GetUserId();
        GachaLogSaveResult result = await gachaLogService.SaveGachaItemsAsync(userId, uidAndItems).ConfigureAwait(false);

        return result.Kind switch
        {
            GachaLogSaveResultKind.Ok => Response<GachaLogSaveResult>.Success($"上传了 {uidAndItems.Uid} 的 {uidAndItems.Items.Count} 条数据，存储了 {result.SaveCount} 条数据", ServerKeys.ServerGachaLogServiceUploadEntrySucceed, result),
            GachaLogSaveResultKind.UidPerUserLimitExceeded => Model.Response.Response.Fail(ReturnCode.TooManyGachaLogUids, "单个胡桃账号最多保存 5 个 Uid 的祈愿记录", ServerKeys.ServerGachaLogServiceInsufficientRecordSlot),
            GachaLogSaveResultKind.InvalidGachaItemDetected => Model.Response.Response.Fail(ReturnCode.InvalidGachaLogItems, "无效的数据，无法保存至云端", ServerKeys.ServerGachaLogServiceInvalidGachaLogData),
            GachaLogSaveResultKind.DatebaseOperationFailed => Model.Response.Response.Fail(ReturnCode.GachaLogDatabaseOperationFailed, "数据异常，无法保存至云端", ServerKeys.ServerGachaLogServiceServerDatabaseError),
            _ => throw new InvalidOperationException(),
        };
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateGachaLogPermission))]
    [HttpGet("Delete")]
    public async Task<IActionResult> DeleteAsync([FromQuery(Name = "Uid")] string uid)
    {
        int count = await gachaLogService.DeleteGachaItemsAsync(this.GetUserId(), uid).ConfigureAwait(false);
        return Response<int>.Success($"删除了 {count} 条记录", "ServerGachaLogServiceDeleteEntrySucceed", count);
    }

    private IActionResult GetGachaLogStatistics<T>(string name)
        where T : class
    {
        T? data = gachaLogService.GetGachaLogStatistics<T>(name);
        return Response<T>.Success("获取祈愿统计数据成功", data!);
    }
}