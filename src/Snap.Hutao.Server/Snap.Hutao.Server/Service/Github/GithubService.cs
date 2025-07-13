// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.OAuth;
using System.Web;

namespace Snap.Hutao.Server.Service.Github;

// Scoped
public class GithubService : IOAuthProvider
{
    private readonly GithubApiService githubApiService;
    private readonly PassportService passportService;
    private readonly DiscordService discordService;
    private readonly GithubOptions githubOptions;
    private readonly AppDbContext appDbContext;

    public GithubService(IServiceProvider serviceProvider)
    {
        githubApiService = serviceProvider.GetRequiredService<GithubApiService>();
        passportService = serviceProvider.GetRequiredService<PassportService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    public Task<string> RequestAuthUrlAsync(string state)
    {
        return Task.FromResult($"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={HttpUtility.UrlEncode(state)}");
    }

    public async Task<bool> RefreshTokenAsync(OAuthBindIdentity identity)
    {
        GithubAccessTokenResponse? accessTokenResponse = await githubApiService.GetAccessTokenByRefreshTokenAsync(identity.RefreshToken).ConfigureAwait(false);
        if (accessTokenResponse is null)
        {
            return false;
        }

        identity.RefreshToken = accessTokenResponse.RefreshToken;
        identity.ExpiresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds();

        await this.appDbContext.OAuthBindIdentities.UpdateAndSaveAsync(identity).ConfigureAwait(false);
        return true;
    }

    public async ValueTask<OAuthResult> HandleOAuthCallbackAsync(string code, OAuthBindState state)
    {
        GithubAccessTokenResponse? accessTokenResponse = await githubApiService.GetAccessTokenByCodeAsync(code).ConfigureAwait(false);
        if (accessTokenResponse is null)
        {
            return OAuthResult.Fail("获取 AccessToken 失败 | Failed to get AccessToken");
        }

        GithubUserResponse? userResponse = await githubApiService.GetUserInfoByAccessTokenAsync(accessTokenResponse.AccessToken).ConfigureAwait(false);
        if (userResponse is null)
        {
            return OAuthResult.Fail("获取 GitHub 用户信息失败 | Failed to get GitHub user information");
        }

        OAuthBindIdentity? identity = await this.appDbContext.OAuthBindIdentities.SingleOrDefaultAsync(b => b.ProviderKind == OAuthProviderKind.Github && b.ProviderId == userResponse.NodeId);
        if (state.UserId is -1)
        {
            // Login mode
            if (identity is null)
            {
                return OAuthResult.Fail("当前 GitHub 账号未绑定胡桃通行证 | The current GitHub account is not bound to Snap Hutao Passport");
            }

            return OAuthResult.LoginSuccess(await this.passportService.CreateTokenResponseAsync(identity.UserId, state.DeviceInfo).ConfigureAwait(false));
        }

        if (identity is not null)
        {
            if (state.UserId != identity.UserId)
            {
                // Already authorized to another user
                return OAuthResult.Fail("当前 GitHub 账号已绑定其他的胡桃通行证 | The current GitHub account is already bound to another Snap Hutao Passport");
            }

            // Already bound to this user, update access token
            identity.RefreshToken = accessTokenResponse.RefreshToken;
            identity.ExpiresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds();
            await this.appDbContext.OAuthBindIdentities.UpdateAndSaveAsync(identity).ConfigureAwait(false);
            return OAuthResult.BindSuccess();
        }

        // First time to bind
        identity = new()
        {
            UserId = state.UserId,
            ProviderKind = OAuthProviderKind.Github,
            ProviderId = userResponse.NodeId,
            DisplayName = userResponse.Login,
            RefreshToken = accessTokenResponse.RefreshToken,
            CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
            ExpiresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds(),
        };

        await this.appDbContext.OAuthBindIdentities.AddAndSaveAsync(identity).ConfigureAwait(false);
        return OAuthResult.BindSuccess();
    }

    public async ValueTask ProcessWorkflowRunEventAsync(WorkflowRun workflowRun)
    {
        Artifacts? artifacts = await githubApiService.GetArtifactsAsync(workflowRun.ArtifactsUrl).ConfigureAwait(false);
        Artifact? artifact = artifacts?.ArtifactList.FirstOrDefault();

        if (artifact is null)
        {
            return;
        }

        GithubWebhookResult githubMessage = new()
        {
            Filename = artifact.Name,
            MarkdownBody = $"""
                ## Snap Hutao Alpha {artifact.Name[17..]}

                [Details](<https://github.com/DGP-Studio/Snap.Hutao/actions/runs/{workflowRun.Id}>)
                [Browser Download Here](<https://github.com/DGP-Studio/Snap.Hutao/actions/runs/{workflowRun.Id}/artifacts/{artifact.Id}>)

                Author: [{workflowRun.HeadCommit.Author.Name}](<https://github.com/{workflowRun.HeadCommit.Author.Name}>)
                Commit: [`{workflowRun.HeadCommit.Id[..7]}`](https://github.com/DGP-Studio/Snap.Hutao/commit/{workflowRun.HeadCommit.Id})
                """,
            Event = GithubWebhookEvent.WorkflowRun,
        };

        await discordService.ReportGithubWebhookAsync(githubMessage);
    }

    public async ValueTask ProcessReleaseEventAsync(Release release)
    {
        Asset? asset = release.Assets.FirstOrDefault();
        if (asset is null)
        {
            return;
        }

        GithubWebhookResult githubMessage = new()
        {
            Filename = asset.Name,
            MarkdownBody = $"""
                ## Snap Hutao {release.Name} 已发布 / Snap Hutao Version {release.Name} is released

                [Release Page]({release.HtmlUrl})
                [Direct Download Link](<{asset.BrowserDownloadUrl}>)

                {release.Body}
                """,
            Event = GithubWebhookEvent.Release,
        };

        await discordService.ReportGithubWebhookAsync(githubMessage);
    }
}