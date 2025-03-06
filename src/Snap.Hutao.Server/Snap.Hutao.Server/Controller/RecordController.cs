// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.Ranking;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "SpiralAbyss")]
public class RecordController : ControllerBase
{
    private readonly RecordService recordService;
    private readonly IRankService rankService;

    public RecordController(RecordService recordService, IRankService rankService)
    {
        this.recordService = recordService;
        this.rankService = rankService;
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> ProcessUploadRecordAsync([FromBody] SimpleRecord record, [FromQuery] bool returningRank = false)
    {
        RecordUploadResult result = await recordService.ProcessUploadAsync(record).ConfigureAwait(false);

        if (returningRank && result < RecordUploadResult.None)
        {
            Rank rank = await rankService.RetriveRankAsync(record.Uid!).ConfigureAwait(false);
            return Response<Rank>.Success("获取排行数据成功", rank);
        }
        else
        {
            return result switch
            {
                RecordUploadResult.OkWithNotFirstAttempt => Model.Response.Response.Success("数据提交成功，但不是本期首次提交，无法获赠祈愿记录上传服务时长", ServerKeys.ServerRecordUploadSuccessButNotFirstTimeAtCurrentSchedule),
                RecordUploadResult.OkWithNoUserNamePresented => Model.Response.Response.Success("数据提交成功，但未绑定胡桃账号，无法获赠祈愿记录上传服务时长", ServerKeys.ServerRecordUploadSuccessButNoPassport),
                RecordUploadResult.OkWithGachaLogExtended => Model.Response.Response.Success("数据提交成功，获赠祈愿记录上传服务时长", ServerKeys.ServerRecordUploadSuccessAndGachaLogServiceTimeExtended),
                RecordUploadResult.OkWithGachaLogNoSuchUser => Model.Response.Response.Success("数据提交成功，但不存在该胡桃账号", ServerKeys.ServerRecordUploadSuccessButNoSuchUser),

                RecordUploadResult.ComputingStatistics => Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", ServerKeys.ServerRecordComputingStatistics),
                RecordUploadResult.UidBanned => Model.Response.Response.Fail(ReturnCode.BannedUid, "Uid 已被数据库封禁", ServerKeys.ServerRecordBannedUid),
                RecordUploadResult.NotCurrentSchedule => Model.Response.Response.Fail(ReturnCode.NotCurrentSchedule, "非当前深渊数据", ServerKeys.ServerRecordNotCurrentSchedule),
                RecordUploadResult.InvalidData => Model.Response.Response.Fail(ReturnCode.InvalidUploadData, "无效的提交数据", ServerKeys.ServerRecordInvalidData),
                RecordUploadResult.ConcurrencyNotSupported => Model.Response.Response.Fail(ReturnCode.PreviousRequestNotCompleted, "该UID的请求尚在处理", ServerKeys.ServerRecordPreviousRequestNotCompleted),
                _ => Model.Response.Response.Success("数据提交成功"),
            };
        }
    }

    [HttpGet("Check")]
    public async Task<IActionResult> CheckUidUploadedAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (recordService.IsStatisticsServiceWorking())
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", ServerKeys.ServerRecordComputingStatistics2);
        }

        if (!int.TryParse(uid, out _) || uid.Length < 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid", ServerKeys.ServerRecordInvalidUid);
        }

        bool uploaded = await recordService.HaveUidUploadedAsync(uid).ConfigureAwait(false);
        return Response<bool>.Success("查询成功", uploaded);
    }

    [HttpGet("Rank")]
    public async Task<IActionResult> RankAsync([FromQuery(Name = "Uid")] string uid)
    {
        if (recordService.IsStatisticsServiceWorking())
        {
            return Model.Response.Response.Fail(ReturnCode.ComputingStatistics, "正在计算统计数据", ServerKeys.ServerRecordComputingStatistics2);
        }

        if (!int.TryParse(uid, out _) || uid.Length < 9)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidQueryString, $"{uid}不是合法的uid", ServerKeys.ServerRecordInvalidUid);
        }

        Rank rank = await rankService.RetriveRankAsync(uid).ConfigureAwait(false);
        return Response<Rank>.Success("获取排行数据成功", rank);
    }
}