// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("OAuth/Github")]
[ServiceFilter(typeof(CountRequests))]
[ApiExplorerSettings(GroupName = "Passport")]
public class GithubAuthorizationController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly AppDbContext appDbContext;
    private readonly GithubOptions githubOptions;
    private readonly PassportService passportService;

    public GithubAuthorizationController(IServiceProvider serviceProvider)
    {
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        passportService = serviceProvider.GetRequiredService<PassportService>();
    }

    [Authorize]
    [HttpGet("RedirectLogin")]
    public async Task<IActionResult> RedirectLoginAsync()
    {
        HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);

        if (user is null)
        {
            return RedirectToError(ReturnCode.UserNameNotExists);
        }

        UserIdentity identity = new()
        {
            UserId = user.Id,
        };
        string state = EncryptState(JsonSerializer.Serialize(identity));
        return Redirect($"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={state}");
    }

    [HttpGet("Authorize")]
    public async Task<IActionResult> HandleAuthorizationCallbackAsync([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return RedirectToError(ReturnCode.InvalidQueryString);
        }

        UserIdentity? userIdentity;
        try
        {
            userIdentity = JsonSerializer.Deserialize<UserIdentity>(DecryptState(state));
            ArgumentNullException.ThrowIfNull(userIdentity);
        }
        catch (Exception)
        {
            return RedirectToError(ReturnCode.InvalidGithubAuthState);
        }

        GithubAccessTokenResponse? accessTokenResponse;
        using (HttpRequestMessage requestMessage = new(HttpMethod.Post, "https://github.com/login/oauth/access_token"))
        {
            requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = githubOptions.ClientId,
                ["client_secret"] = githubOptions.ClientSecret,
                ["code"] = code,
            });

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage))
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToError(ReturnCode.InternalGithubAuthException);
                }

                accessTokenResponse = await responseMessage.Content.ReadFromJsonAsync<GithubAccessTokenResponse>();

                if (accessTokenResponse is null)
                {
                    return RedirectToError(ReturnCode.InternalGithubAuthException);
                }
            }
        }

        GithubUserResponse? userResponse;
        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://api.github.com/user"))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResponse.AccessToken);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage))
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToError(ReturnCode.InternalGithubAuthException);
                }

                userResponse = await responseMessage.Content.ReadFromJsonAsync<GithubUserResponse>();

                if (userResponse is null)
                {
                    return RedirectToError(ReturnCode.InternalGithubAuthException);
                }
            }
        }

        if (await appDbContext.GithubIdentities.SingleOrDefaultAsync(g => g.Id == userResponse.Id).ConfigureAwait(false) is { } identity)
        {
            if (identity.UserId != userIdentity.UserId)
            {
                // Already authorized to another user
                return RedirectToError(ReturnCode.GithubAlreadyAuthorized);
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

        // Authorized
        string token = passportService.CreateTokenByUserId(identity.UserId);
        return Redirect($"https://passport.snapgenshin.cn/api/login?token={token}");
    }

    [Authorize]
    [HttpGet("UnAuthorize")]
    public async Task<IActionResult> UnAuthorizeAsync()
    {
        int userId = this.GetUserId();
        int count = await appDbContext.GithubIdentities.Where(g => g.UserId == userId).ExecuteDeleteAsync().ConfigureAwait(false);
        return Response<UnAuthorizeResult>.Success("操作完成", new() { Count = count });
    }

    [Authorize]
    [HttpGet("AuthorizationStatus")]
    public async Task<IActionResult> GetAuthorizationStatusAsync()
    {
        int userId = this.GetUserId();
        bool isAuthorized = await appDbContext.GithubIdentities.AnyAsync(g => g.UserId == userId).ConfigureAwait(false);
        return Response<IsAuthorizedResult>.Success("查询成功", new() { IsAuthorized = isAuthorized });
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

    private RedirectResult RedirectToError(ReturnCode errorCode)
    {
        return Redirect($"https://passport.snapgenshin.cn/auth/error?code={errorCode:D}");
    }

    private sealed class UserIdentity
    {
        public int UserId { get; set; }
    }

    private sealed class GithubAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = default!;

        [JsonPropertyName("refresh_token_expires_in")]
        public long RefreshTokenExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = default!;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = default!;
    }

    private sealed class GithubUserResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; } = default!;
    }

    private sealed class UnAuthorizeResult
    {
        public int Count { get; set; }
    }

    private sealed class IsAuthorizedResult
    {
        public bool IsAuthorized { get; set; }
    }
}