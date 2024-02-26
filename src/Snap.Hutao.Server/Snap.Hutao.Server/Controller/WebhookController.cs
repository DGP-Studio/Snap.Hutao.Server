// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Snap.Hutao.Server.Model.Afdian;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Afdian;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.GachaLog;
using static System.Net.WebRequestMethods;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class WebhookController : ControllerBase
{
    private readonly AfdianWebhookService afdianWebhookService;
    private readonly DiscordService discordService;
    private readonly GithubOptions githubOptions;
    private readonly HttpClient httpClient;

    public WebhookController(IServiceProvider serviceProvider)
    {
        afdianWebhookService = serviceProvider.GetRequiredService<AfdianWebhookService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
    }

    [HttpGet("Incoming/Afdian")]
    [HttpPost("Incoming/Afdian")]
    public async Task<IActionResult> IncomingAfdianAsync([FromBody] AfdianResponse<OrderWrapper> request)
    {
        await afdianWebhookService.ProcessIncomingOrderAsync(request.Data.Order).ConfigureAwait(false);
        return new JsonResult(new AfdianResponse() { ErrorCode = 200, ErrorMessage = string.Empty });
    }

    [HttpPost("Incoming/Github")]
    public async Task<IActionResult> IncomingGithubAsync([FromBody] GithubRequest request)
    {
        string? githubEvent = HttpContext.Request.Headers["X-GitHub-Event"];

        if (githubEvent == "ping")
        {
            return new JsonResult(new GithubResponse() { Message = "pong" });
        }

        if (githubEvent == "workflow_run" && request is { Action: "completed", WorkflowRun.PullRequests.Count: 0 })
        {
            await ProcessWorkflowRunEventAsync(request.WorkflowRun).ConfigureAwait(false);
            return new JsonResult(new GithubResponse() { Message = "Alpha sent to Discord" });
        }
        else if (githubEvent == "release" && request is { Action: "released", Release: { } })
        {
            await ProcessReleaseEventAsync(request.Release).ConfigureAwait(false);
            return new JsonResult(new GithubResponse() { Message = "Release sent to Discord" });
        }

        return new JsonResult(new GithubResponse() { Message = "Skip" });
    }

    private async ValueTask ProcessWorkflowRunEventAsync(WorkflowRun workflowRun)
    {
        Artifact? artifact = default;
        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, workflowRun.ArtifactsUrl))
        {
            requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
            requestMessage.Headers.Authorization = new("Bearer", githubOptions.Token);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    Artifacts? artifacts = await responseMessage.Content.ReadFromJsonAsync<Artifacts>();

                    if (artifacts is not null)
                    {
                        artifact = artifacts.ArtifactList.FirstOrDefault();
                    }
                }
            }
        }

        if (artifact is null)
        {
            return;
        }

        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, artifact.ArchiveDownloadUrl))
        {
            requestMessage.Headers.Authorization = new("Bearer", githubOptions.Token);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                responseMessage.EnsureSuccessStatusCode();

                Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                GithubMessage githubMessage = new()
                {
                    Filename = artifact.Name,
                    Stream = stream,
                    MarkdownBody = $"""
                    ## Snap Hutao Alpha {artifact.Name[17..]}

                    [Browser Download Here](https://github.com/DGP-Studio/Snap.Hutao/actions/runs/{workflowRun.Id}/artifacts/{artifact.Id})



                    `[{workflowRun.HeadCommit.Id[..7]}](https://github.com/DGP-Studio/Snap.Hutao/commit/{workflowRun.HeadCommit.Id})` {workflowRun.HeadCommit.Message}

                    by [{workflowRun.HeadCommit.Author.Name}](https://github.com/{workflowRun.HeadCommit.Author.Name})
                    """,
                    Type = GithubMessageType.WorkflowRun,
                };

                await discordService.ReportGithubWebhookAsync(githubMessage);

                stream.Dispose();
            }
        }
    }

    private async ValueTask ProcessReleaseEventAsync(Release release)
    {
        Asset? asset = release.Assets.FirstOrDefault();
        if (asset is null)
        {
            return;
        }

        using (HttpResponseMessage responseMessage = await httpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
        {
            responseMessage.EnsureSuccessStatusCode();

            Stream stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            GithubMessage githubMessage = new()
            {
                Filename = asset.Name,
                Stream = stream,
                MarkdownBody = $"""
                ## Snap Hutao {release.Name} 版本已发布/ Snap Hutao Version {release.Name} is released

                [Release Page]({release.HtmlUrl})

                [Direct Download Link]({asset.BrowserDownloadUrl})



                {release.Body}
                """,
                Type = GithubMessageType.Release,
            };

            await discordService.ReportGithubWebhookAsync(githubMessage);

            stream.Dispose();
        }
    }
}