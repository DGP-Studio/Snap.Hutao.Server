// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Github;
using System.IdentityModel.Tokens.Jwt;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Passport")]
public class GithubController : ControllerBase
{
    private readonly GithubService githubService;

    public GithubController(IServiceProvider serviceProvider)
    {
        githubService = serviceProvider.GetRequiredService<GithubService>();
    }

    [HttpPost("Webhook")]
    public async Task<IActionResult> ProcessWebhookAsync([FromBody] GithubWebhookRequest request)
    {
        switch (HttpContext.Request.Headers["X-GitHub-Event"])
        {
            case "ping":
                return new JsonResult(new GithubWebhookResponse { Message = "pong" });
            case "workflow_run" when request is { Action: "completed", WorkflowRun.Name: "Snap Hutao Alpha", WorkflowRun.PullRequests.Count: 0 }:
                await githubService.ProcessWorkflowRunEventAsync(request.WorkflowRun).ConfigureAwait(false);
                return new JsonResult(new GithubWebhookResponse { Message = "Alpha sent to Discord" });
            case "release" when request is { Action: "released", Release: not null }:
                await githubService.ProcessReleaseEventAsync(request.Release).ConfigureAwait(false);
                return new JsonResult(new GithubWebhookResponse { Message = "Release sent to Discord" });
            default:
                return new JsonResult(new GithubWebhookResponse { Message = "Skip" });
        }
    }
}