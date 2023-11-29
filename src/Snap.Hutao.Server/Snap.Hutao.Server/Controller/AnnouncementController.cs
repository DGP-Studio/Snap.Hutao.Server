// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Service.Announcement;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AnnouncementController : ControllerBase
{
    private readonly AnnouncementService announcementService;

    public AnnouncementController(AnnouncementService announcementService)
    {
        this.announcementService = announcementService;
    }

    [HttpPost("List")]
    public async Task<IActionResult> List([FromBody] HashSet<long> excludedIds)
    {
        List<EntityAnnouncement> results = await announcementService
            .GetAnnouncementsAsync(Request.Headers.UserAgent, "ALL", excludedIds)
            .ConfigureAwait(false);

        return Model.Response.Response<List<EntityAnnouncement>>.Success("获取公告成功", results);
    }
}