// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Web;

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
    private readonly IOptionsMonitor<JwtBearerOptions> jwtBearerOptions;
    private readonly ILogger<GithubAuthorizationController> logger;
    private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

    public GithubAuthorizationController(IServiceProvider serviceProvider)
    {
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        passportService = serviceProvider.GetRequiredService<PassportService>();
        logger = serviceProvider.GetRequiredService<ILogger<GithubAuthorizationController>>();
        jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
    }

    [HttpGet("RedirectLogin")]
    public async Task<IActionResult> RedirectLoginAsync([FromQuery(Name = "token")] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToError(ReturnCode.InvalidQueryString);
        }

        JwtBearerOptions options = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        jwtSecurityTokenHandler.ValidateToken(token, options.TokenValidationParameters, out SecurityToken validatedToken);
        if (validatedToken is not JwtSecurityToken jwtSecurityToken)
        {
            return RedirectToError(ReturnCode.LoginFail);
        }

        int userId;
        try
        {
            userId = int.Parse(jwtSecurityToken.Claims.Single(c => c.Type == PassportClaimTypes.UserId).Value);
        }
        catch
        {
            return RedirectToError(ReturnCode.UserNameNotExists);
        }

        HutaoUser? user = await appDbContext.Users.SingleOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

        if (user is null)
        {
            return RedirectToError(ReturnCode.UserNameNotExists);
        }

        UserIdentity identity = new()
        {
            UserId = user.Id,
        };

        string state = HttpUtility.UrlEncode(EncryptState(JsonSerializer.Serialize(identity)));
        return Redirect($"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={state}");
    }

    [HttpGet("Authorize")]
    public async Task<IActionResult> HandleAuthorizationCallbackAsync([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            return RedirectToError(ReturnCode.GithubAuthrizationCanceled);
        }

        if (string.IsNullOrEmpty(state))
        {
            return RedirectToError(ReturnCode.InvalidQueryString);
        }

        UserIdentity? userIdentity;
        try
        {
            logger.LogInformation("State: {State}", state);
            userIdentity = JsonSerializer.Deserialize<UserIdentity>(DecryptState(state));
            ArgumentNullException.ThrowIfNull(userIdentity);
        }
        catch (Exception)
        {
            return RedirectToError(ReturnCode.InvalidGithubAuthState);
        }

        GithubAccessTokenResponse? accessTokenResponse;
        string accessTokenQuery = $"client_id={githubOptions.ClientId}&client_secret={githubOptions.ClientSecret}&code={code}";
        using (HttpRequestMessage requestMessage = new(HttpMethod.Post, $"https://github.com/login/oauth/access_token?{accessTokenQuery}"))
        {
            requestMessage.Headers.Accept.Add(new("application/json"));
            requestMessage.Headers.Authorization = new("token", code);

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

        logger.LogInformation("AccessToken: {Token}", accessTokenResponse.AccessToken);

        GithubUserResponse? userResponse;
        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://api.github.com/user"))
        {
            requestMessage.Headers.Accept.Add(new("application/vnd.github+json"));
            requestMessage.Headers.UserAgent.ParseAdd("Snap Hutao Server/1.0");
            requestMessage.Headers.Authorization = new("Bearer", accessTokenResponse.AccessToken);

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
        return Redirect($"https://passport.snapgenshin.cn/api/users/login?token={token}");
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