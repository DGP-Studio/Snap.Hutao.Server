// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Model.Response;
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

    public GithubService(IServiceProvider serviceProvider)
    {
        githubApiService = serviceProvider.GetRequiredService<GithubApiService>();
        passportService = serviceProvider.GetRequiredService<PassportService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    public string CreateStateForUser(HutaoUser user)
    {
        UserIdentity identity = new() { UserId = user.Id };
        return HttpUtility.UrlEncode(EncryptState(JsonSerializer.Serialize(identity)));
    }

    public async ValueTask<OAuthResult> HandleOAuthCallbackAsync(string code, OAuthBindState state)
    {
        GithubAccessTokenResponse? accessTokenResponse = await githubApiService.GetAccessTokenByCodeAsync(code).ConfigureAwait(false);
        if (accessTokenResponse is null)
        {
            return OAuthResult.Fail("ServerOAuthInternalException");
        }

        GithubUserResponse? userResponse = await githubApiService.GetUserInfoByAccessTokenAsync(accessTokenResponse.AccessToken).ConfigureAwait(false);
        if (userResponse is null)
        {
            return OAuthResult.Fail("ServerOAuthInternalException");
        }

        OAuthBindIdentity? identity = await this.appDbContext.OAuthBindIdentities.SingleOrDefaultAsync(b => b.ProviderKind == OAuthProviderKind.Github && b.ProviderId == userResponse.NodeId);
        if (state.UserId is -1)
        {
            // Login mode
            if (identity is null)
            {
                // TODO: Or register new account
                return OAuthResult.Fail("ServerOAuthNotBinded");
            }

            return OAuthResult.LoginSuccess(await this.passportService.CreateTokenResponseAsync(identity.UserId).ConfigureAwait(false));
        }

        if (identity is not null)
        {
            if (state.UserId != identity.UserId)
            {
                // Already authorized to another user
                return OAuthResult.Fail("ServerOAuthAlreadyBinded");
            }

            // Already bound to this user, update access token
            identity.RefreshToken = accessTokenResponse.RefreshToken;
            identity.ExpiresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds();
            await this.appDbContext.OAuthBindIdentities.UpdateAndSaveAsync(identity).ConfigureAwait(false);
            return OAuthResult.BindSuccess();
        }

        // First time to bind
        identity = new OAuthBindIdentity
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

    [Obsolete]
    public async ValueTask<AuthorizeResult> HandleAuthorizationCallbackAsync(string code, string state)
    {
        UserIdentity? userIdentity;
        try
        {
            userIdentity = JsonSerializer.Deserialize<UserIdentity>(DecryptState(state));
            ArgumentNullException.ThrowIfNull(userIdentity);
        }
        catch (Exception)
        {
            return new() { Success = false, ReturnCode = ReturnCode.InvalidGithubAuthState };
        }

        GithubAccessTokenResponse? accessTokenResponse = await githubApiService.GetAccessTokenByCodeAsync(code).ConfigureAwait(false);
        if (accessTokenResponse is null)
        {
            return new() { Success = false, ReturnCode = ReturnCode.InternalGithubAuthException };
        }

        GithubUserResponse? userResponse = await githubApiService.GetUserInfoByAccessTokenAsync(accessTokenResponse.AccessToken).ConfigureAwait(false);
        if (userResponse is null)
        {
            return new() { Success = false, ReturnCode = ReturnCode.InternalGithubAuthException };
        }

        if (await appDbContext.GithubIdentities.SingleOrDefaultAsync(g => g.Id == userResponse.Id).ConfigureAwait(false) is { } identity)
        {
            if (identity.UserId != userIdentity.UserId)
            {
                // Already authorized to another user
                return new() { Success = false, ReturnCode = ReturnCode.GithubAlreadyAuthorized };
            }
        }
        else
        {
            identity = new GithubIdentity
            {
                Id = userResponse.Id,
                NodeId = userResponse.NodeId,
                UserId = userIdentity.UserId,
                RefreshToken = accessTokenResponse.RefreshToken,
                ExipresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds(),
            };

            // First time to authorize
            await appDbContext.GithubIdentities.AddAndSaveAsync(identity).ConfigureAwait(false);
        }

        return new() { Success = true, Token = passportService.CreateTokenByUserId(identity.UserId) };
    }

    public async ValueTask<AuthorizationStatus> GetAuthorizationStatusAsync(int userId)
    {
        GithubIdentity? identity = await appDbContext.GithubIdentities.SingleOrDefaultAsync(g => g.UserId == userId).ConfigureAwait(false);

        if (identity is null)
        {
            return new() { IsAuthorized = false };
        }

        GithubAccessTokenResponse? accessTokenResponse = await githubApiService.GetAccessTokenByRefreshTokenAsync(identity.RefreshToken).ConfigureAwait(false);
        if (accessTokenResponse is not null)
        {
            identity.RefreshToken = accessTokenResponse.RefreshToken;
            identity.ExipresAt = (DateTimeOffset.Now + TimeSpan.FromSeconds(accessTokenResponse.RefreshTokenExpiresIn)).ToUnixTimeSeconds();

            await appDbContext.GithubIdentities.UpdateAndSaveAsync(identity).ConfigureAwait(false);
        }

        return new AuthorizationStatus { IsAuthorized = true, AccessToken = accessTokenResponse?.AccessToken };
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

    private string EncryptState(string state)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = githubOptions.StateEncryptKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream targetStream = new())
            {
                using (ICryptoTransform transform = aes.CreateEncryptor())
                {
                    using (CryptoStream cryptoStream = new(targetStream, transform, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new(cryptoStream, leaveOpen: true))
                        {
                            streamWriter.Write(state);
                            streamWriter.Flush();
                        }

                        cryptoStream.FlushFinalBlock();
                    }
                }

                return Convert.ToBase64String(targetStream.ToArray());
            }
        }
    }

    private string DecryptState(string state)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = githubOptions.StateEncryptKey;
            aes.IV = new byte[16];
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream sourceStream = new(Convert.FromBase64String(state)))
            {
                using (ICryptoTransform transform = aes.CreateDecryptor())
                {
                    using (CryptoStream cryptoStream = new(sourceStream, transform, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new(cryptoStream, leaveOpen: true))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public async Task<string> RequestAuthUrlAsync(string state)
    {
        return $"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={HttpUtility.UrlEncode(state)}";
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
}