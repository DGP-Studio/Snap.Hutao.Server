// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.RoleCombat;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.RoleCombat;
using Snap.Hutao.Server.Service.RoleCombat;
using System.Collections.Concurrent;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
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

    [HttpPost("Upload")]
    public async Task<IActionResult> Upload([FromBody] SimpleRoleCombatRecord record)
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
            return Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理", ServerKeys.ServerRecordPreviousRequestNotCompleted);
        }

        using (IDbContextTransaction transaction = await appDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
        {
            RoleCombatRecord? exist = await appDbContext.RoleCombatRecords.SingleOrDefaultAsync(r => r.Uid == record.Uid).ConfigureAwait(false);

            if (exist is not null)
            {
                await appDbContext.RoleCombatAvatars.Where(a => a.RecordId == exist.PrimaryId).ExecuteDeleteAsync().ConfigureAwait(false);
            }

            await appDbContext.RoleCombatRecords.Where(r => r.Uid == record.Uid).ExecuteDeleteAsync().ConfigureAwait(false);

            RoleCombatRecord newRecord = new()
            {
                Uid = record.Uid,
                Uploader = record.Identity,
                UploadTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            };
            await appDbContext.RoleCombatRecords.AddAndSaveAsync(newRecord).ConfigureAwait(false);

            long recordId = newRecord.PrimaryId;
            List<RoleCombatAvatar> avatars = record.BackupAvatars.Select(id => new RoleCombatAvatar() { AvatarId = id, RecordId = recordId }).ToList();
            await appDbContext.RoleCombatAvatars.AddRangeAndSaveAsync(avatars).ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);
        }

        if (!UploadingUids.TryRemove(record.Uid, out _))
        {
            return Response.Fail(ReturnCode.InternalStateException, "提交状态异常", ServerKeys.ServerRecordInternalException);
        }

        return Response.Success("数据提交成功");
    }

    [HttpGet("Statistics")]
    public IActionResult GetStatistics()
    {
        int scheduleId = RoleCombatScheduleId.GetForNow();

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