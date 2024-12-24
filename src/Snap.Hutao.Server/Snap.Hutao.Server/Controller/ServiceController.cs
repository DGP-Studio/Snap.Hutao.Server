﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service.Announcement;
using Snap.Hutao.Server.Service.Distribution;
using Snap.Hutao.Server.Service.Expire;
using Snap.Hutao.Server.Service.GachaLog;

namespace Snap.Hutao.Server.Controller;

[Authorize]
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
[ServiceFilter(typeof(ValidateMaintainPermission))]
[ApiExplorerSettings(GroupName = "Services")]
public class ServiceController : ControllerBase
{
    private readonly GachaLogExpireService gachaLogExpireService;
    private readonly CdnExpireService cdnExpireService;
    private readonly AnnouncementService announcementService;
    private readonly IServiceProvider serviceProvider;

    public ServiceController(IServiceProvider serviceProvider)
    {
        this.gachaLogExpireService = serviceProvider.GetRequiredService<GachaLogExpireService>();
        cdnExpireService = serviceProvider.GetRequiredService<CdnExpireService>();
        announcementService = serviceProvider.GetRequiredService<AnnouncementService>();
        this.serviceProvider = serviceProvider;
    }

    [HttpGet("Statistics/Run")]
    public async Task<IActionResult> RunStatisticsAsync()
    {
        LegacyStatisticsRefreshJob job = serviceProvider.GetRequiredService<LegacyStatisticsRefreshJob>();
        await job.Execute(default!).ConfigureAwait(false);
        return Model.Response.Response.Success("操作成功");
    }

    [HttpGet("GachaLog/Compensation")]
    public async Task<IActionResult> GachaLogCompensationAsync([FromQuery] int days)
    {
        DateTimeOffset target = await this.gachaLogExpireService.ExtendTermForAllUsersAsync(days).ConfigureAwait(false);
        return Model.Response.Response.Success($"操作成功，增加了 {days} 天时长，到期时间: {target}");
    }

    [HttpGet("GachaLog/Designation")]
    public async Task<IActionResult> GachaLogDesignationAsync([FromQuery] string userName, [FromQuery] int days)
    {
        TermExtendResult result = await this.gachaLogExpireService.ExtendTermForUserNameAsync(userName, days).ConfigureAwait(false);
        return result.Kind switch
        {
            TermExtendResultKind.Ok => Model.Response.Response.Success($"操作成功，增加了 {days} 天时长，到期时间: {result.ExpiredAt}"),
            TermExtendResultKind.NoSuchUser => Model.Response.Response.Fail(ReturnCode.UserNameNotExists, $"用户名不存在"),
            _ => Model.Response.Response.Fail(ReturnCode.GachaLogExtendDbException, $"数据库错误"),
        };
    }

    [HttpGet("Distribution/Compensation")]
    public async Task<IActionResult> DistributionCompensationAsync([FromQuery] int days)
    {
        DateTimeOffset target = await this.cdnExpireService.ExtendTermForAllUsersAsync(days).ConfigureAwait(false);
        return Model.Response.Response.Success($"操作成功，增加了 {days} 天时长，到期时间: {target}");
    }

    [HttpGet("Distribution/Designation")]
    public async Task<IActionResult> DistributionDesignationAsync([FromQuery] string userName, [FromQuery] int days)
    {
        TermExtendResult result = await this.cdnExpireService.ExtendTermForUserNameAsync(userName, days).ConfigureAwait(false);
        return result.Kind switch
        {
            TermExtendResultKind.Ok => Model.Response.Response.Success($"操作成功，增加了 {days} 天时长，到期时间: {result.ExpiredAt}"),
            TermExtendResultKind.NoSuchUser => Model.Response.Response.Fail(ReturnCode.UserNameNotExists, $"用户名不存在"),
            _ => Model.Response.Response.Fail(ReturnCode.GachaLogExtendDbException, $"数据库错误"),
        };
    }

    [HttpPost("Announcement/Upload")]
    public async Task<IActionResult> UploadAnnouncementAsync([FromBody] HutaoUploadAnnouncement announcement)
    {
        await announcementService.ProcessUploadAnnouncementAsync(announcement).ConfigureAwait(false);
        return Model.Response.Response.Success("上传公告成功");
    }
}