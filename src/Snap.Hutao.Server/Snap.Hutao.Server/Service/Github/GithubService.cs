// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.OAuth;
using System.Security.Cryptography;
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
    private readonly UserManager<HutaoUser> userManager;

    public GithubService(IServiceProvider serviceProvider)
    {
        githubApiService = serviceProvider.GetRequiredService<GithubApiService>();
        passportService = serviceProvider.GetRequiredService<PassportService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        userManager = serviceProvider.GetRequiredService<UserManager<HutaoUser>>();
    }

    public Task<string> RequestAuthUrlAsync(string state)
    {
        string encodedState = HttpUtility.UrlEncode(state);
        return Task.FromResult($"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={encodedState}&scope=read:user%20user:email");
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

        OAuthBindIdentity? identity = await appDbContext.OAuthBindIdentities
            .SingleOrDefaultAsync(b => b.ProviderKind == OAuthProviderKind.Github && b.ProviderId == userResponse.NodeId)
            .ConfigureAwait(false);

        if (state.UserId is -1)
        {
            if (identity is not null)
            {
                await UpdateIdentityAsync(identity, accessTokenResponse, userResponse.Login).ConfigureAwait(false);
                TokenResponse existingToken = await passportService.CreateTokenResponseAsync(identity.UserId, state.DeviceInfo).ConfigureAwait(false);
                return OAuthResult.LoginSuccess(existingToken);
            }

            (string Email, bool Verified)? emailResult = await ResolveEmailAsync(accessTokenResponse, userResponse).ConfigureAwait(false);
            if (emailResult is null)
            {
                return OAuthResult.Fail("无法获取 GitHub 邮箱地址 | Failed to get GitHub email address");
            }

            (string email, bool verified) = emailResult.Value;
            if (!verified)
            {
                return OAuthResult.Fail("GitHub 邮箱未验证，无法完成登录或注册 | The GitHub email address is unverified and cannot be used for login or registration");
            }
            HutaoUser? user = await userManager.FindByNameAsync(email).ConfigureAwait(false);
            TokenResponse tokenResponse;

            if (user is null)
            {
                Passport passport = new()
                {
                    UserName = email,
                    Password = GenerateTemporaryPassword(),
                };

                PassportResult registerResult = await passportService.RegisterAsync(passport, state.DeviceInfo).ConfigureAwait(false);
                if (!registerResult.Success)
                {
                    return OAuthResult.Fail($"通过 GitHub 注册失败: {registerResult.Message} | Failed to register via GitHub: {registerResult.Message}");
                }

                tokenResponse = registerResult.Token;
                user = await userManager.FindByNameAsync(email).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("GitHub 注册成功但未找到对应用户 | GitHub registration succeeded but user not found");
            }
            else
            {
                tokenResponse = await passportService.CreateTokenResponseAsync(user, state.DeviceInfo).ConfigureAwait(false);
            }

            await EnsureUserEmailAsync(user, email, verified).ConfigureAwait(false);

            OAuthBindIdentity newIdentity = new()
            {
                UserId = user.Id,
                ProviderKind = OAuthProviderKind.Github,
                ProviderId = userResponse.NodeId,
                DisplayName = userResponse.Login,
                RefreshToken = accessTokenResponse.RefreshToken,
                CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                ExpiresAt = CalculateExpiresAt(accessTokenResponse),
            };

            await appDbContext.OAuthBindIdentities.AddAndSaveAsync(newIdentity).ConfigureAwait(false);
            return OAuthResult.LoginSuccess(tokenResponse);
        }

        if (identity is not null)
        {
            if (state.UserId != identity.UserId)
            {
                return OAuthResult.Fail("当前 GitHub 账号已绑定其他的胡桃通行证 | The current GitHub account is already bound to another Snap Hutao Passport");
            }

            await UpdateIdentityAsync(identity, accessTokenResponse, userResponse.Login).ConfigureAwait(false);
            return OAuthResult.BindSuccess();
        }

        OAuthBindIdentity bindIdentity = new()
        {
            UserId = state.UserId,
            ProviderKind = OAuthProviderKind.Github,
            ProviderId = userResponse.NodeId,
            DisplayName = userResponse.Login,
            RefreshToken = accessTokenResponse.RefreshToken,
            CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
            ExpiresAt = CalculateExpiresAt(accessTokenResponse),
        };

        await appDbContext.OAuthBindIdentities.AddAndSaveAsync(bindIdentity).ConfigureAwait(false);
        return OAuthResult.BindSuccess();
    }

    private async ValueTask<(string Email, bool Verified)?> ResolveEmailAsync(GithubAccessTokenResponse accessTokenResponse, GithubUserResponse userResponse)
    {
        List<GithubEmailAddress>? emailAddresses = await githubApiService.GetUserEmailsByAccessTokenAsync(accessTokenResponse.AccessToken).ConfigureAwait(false);
        if (emailAddresses is null || emailAddresses.Count == 0)
        {
            return null;
        }

        GithubEmailAddress? selectedEmail = emailAddresses
            .Where(address => address.Verified)
            .OrderByDescending(address => address.Primary)
            .ThenBy(address => address.Email, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (selectedEmail is not null)
        {
            return (selectedEmail.Email, true);
        }

        if (!string.IsNullOrEmpty(userResponse.Email))
        {
            GithubEmailAddress? matchingEmail = emailAddresses.FirstOrDefault(address => string.Equals(address.Email, userResponse.Email, StringComparison.OrdinalIgnoreCase));
            if (matchingEmail is not null && matchingEmail.Verified)
            {
                return (matchingEmail.Email, true);
            }
        }

        return null;
    }

    private async ValueTask EnsureUserEmailAsync(HutaoUser user, string email, bool verified)
    {
        bool shouldUpdate = false;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email;
            shouldUpdate = true;
        }

        if (verified && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            shouldUpdate = true;
        }

        if (shouldUpdate)
        {
            await userManager.UpdateAsync(user).ConfigureAwait(false);
        }
    }

    private async ValueTask UpdateIdentityAsync(OAuthBindIdentity identity, GithubAccessTokenResponse accessTokenResponse, string displayName)
    {
        identity.DisplayName = displayName;
        identity.RefreshToken = accessTokenResponse.RefreshToken;
        identity.ExpiresAt = CalculateExpiresAt(accessTokenResponse);

        await appDbContext.OAuthBindIdentities.UpdateAndSaveAsync(identity).ConfigureAwait(false);
    }

    private static long CalculateExpiresAt(GithubAccessTokenResponse accessTokenResponse)
    {
        return (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds();
    }

    private static string GenerateTemporaryPassword()
    {
        Span<byte> buffer = stackalloc byte[48];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer);
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