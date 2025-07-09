// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Github;
using Snap.Hutao.Server.Service.OAuth;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "OAuth")]
public class OAuthController : ControllerBase
{
    private readonly IServiceProvider serviceProvider;
    private readonly AppDbContext appDbContext;
    private readonly AppOptions appOptions;

    public OAuthController(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        this.appOptions = serviceProvider.GetRequiredService<AppOptions>();
    }

    [HttpGet("Auth")]
    public async Task<IActionResult> RequestAuthAsync([FromQuery(Name = "provider")] string providerKind, [FromQuery(Name = "callback")] string callbackUri)
    {
        if (string.IsNullOrEmpty(providerKind))
        {
            return BadRequest("Invalid OAuth provider kind.");
        }

        if (string.IsNullOrEmpty(callbackUri))
        {
            return BadRequest("Callback URL is required.");
        }

        if (!Enum.TryParse(providerKind, true, out OAuthProviderKind kind))
        {
            return BadRequest("Unsupported OAuth provider kind.");
        }

        OAuthBindState state;
        if (this.TryGetUserId(out int userId))
        {
            state = new(userId, callbackUri);
        }
        else
        {
            state = new(callbackUri);
        }

        string encryptedState = EncryptState(JsonSerializer.Serialize(state));

        IOAuthProvider provider = this.serviceProvider.GetRequiredKeyedService<IOAuthProvider>(kind);
        return Redirect(await provider.RequestAuthUrlAsync(encryptedState).ConfigureAwait(false));
    }

    [Authorize]
    [HttpGet("Unbind")]
    public async Task<IActionResult> UnbindAsync([FromQuery(Name = "provider")] string providerKind)
    {
        if (string.IsNullOrEmpty(providerKind))
        {
            return BadRequest("Invalid OAuth provider kind.");
        }

        if (!Enum.TryParse(providerKind, true, out OAuthProviderKind kind))
        {
            return BadRequest("Unsupported OAuth provider kind.");
        }

        int userId = this.GetUserId();
        await this.appDbContext.OAuthBindIdentities.Where(b => b.UserId == userId && b.ProviderKind == kind)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        return Model.Response.Response.Success("解绑成功");
    }

    [Authorize]
    [HttpGet("Status")]
    public async Task<IActionResult> GetBindStatusAsync([FromQuery(Name = "provider")] string providerKind)
    {
        if (string.IsNullOrEmpty(providerKind))
        {
            return BadRequest("Invalid OAuth provider kind.");
        }

        if (!Enum.TryParse(providerKind, true, out OAuthProviderKind kind))
        {
            return BadRequest("Unsupported OAuth provider kind.");
        }

        int userId = this.GetUserId();
        OAuthBindIdentity? bindIdentity = await this.appDbContext.OAuthBindIdentities
            .Where(b => b.UserId == userId && b.ProviderKind == kind)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

        if (bindIdentity is null)
        {
            return Response<OAuthBindStatus>.Success("获取成功", OAuthBindStatus.NotBinded());
        }

        return Response<OAuthBindStatus>.Success("获取成功", OAuthBindStatus.Binded(bindIdentity));
    }

    [HttpGet("Callback/GitHub")]
    public async Task<IActionResult> BindGitHubCallbackAsync([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization canceled.");
        }

        if (string.IsNullOrEmpty(state))
        {
            return BadRequest("Authorization failed.");
        }

        OAuthBindState? decryptedState = JsonSerializer.Deserialize<OAuthBindState>(DecryptState(state));
        if (decryptedState is null)
        {
            return BadRequest("Invalid state.");
        }

        // TODO: We can unify the callback handling function for different providers in the interface
        // But the callback handling function signatures may have `params object` which cannot be fixed at compile time.
        GithubService githubService = this.serviceProvider.GetRequiredService<GithubService>();
        OAuthResult result = await githubService.HandleOAuthCallbackAsync(code, decryptedState).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        if (result.IsBind)
        {
            return Redirect(decryptedState.CallbackUri);
        }

        ArgumentNullException.ThrowIfNull(result.TokenResponse);
        return this.Redirect($"{decryptedState.CallbackUri}?access_token={result.TokenResponse.AccessToken}&refresh_token={result.TokenResponse.RefreshToken}&expires_in={result.TokenResponse.ExpiresIn}");
    }

    private string EncryptState(string state)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = appOptions.OAuthStateEncryptKey;
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
            aes.Key = appOptions.OAuthStateEncryptKey;
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
}