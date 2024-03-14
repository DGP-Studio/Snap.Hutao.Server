// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Option;

namespace Snap.Hutao.Server.Service.Github;

// Transient
public class GithubApiService
{
    private readonly GithubOptions githubOptions;
    private readonly HttpClient httpClient;

    public GithubApiService(IServiceProvider serviceProvider)
    {
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
    }

    public async ValueTask<Artifacts?> GetArtifactsAsync(string artifactsUrl)
    {
        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, artifactsUrl))
        {
            requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
            requestMessage.Headers.UserAgent.ParseAdd("Snap Hutao Server/1.0");
            requestMessage.Headers.Authorization = new("Bearer", githubOptions.Token);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return default;
                }

                return await responseMessage.Content.ReadFromJsonAsync<Artifacts>();
            }
        }
    }

    public async ValueTask<GithubUserResponse?> GetUserInfoByAccessTokenAsync(string accessToken)
    {
        using (HttpRequestMessage requestMessage = new(HttpMethod.Get, "https://api.github.com/user"))
        {
            requestMessage.Headers.Accept.Add(new("application/vnd.github+json"));
            requestMessage.Headers.UserAgent.ParseAdd("Snap Hutao Server/1.0");
            requestMessage.Headers.Authorization = new("Bearer", accessToken);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage))
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return default;
                }

                return await responseMessage.Content.ReadFromJsonAsync<GithubUserResponse>();
            }
        }
    }

    public async ValueTask<GithubAccessTokenResponse?> GetAccessTokenByRefreshTokenAsync(string refreshToken)
    {
        string query = $"client_id={githubOptions.ClientId}&client_secret={githubOptions.ClientSecret}&grant_type=refresh_token&refresh_token={refreshToken}";
        return await GetAccessTokenCoreAsync(query, refreshToken).ConfigureAwait(false);
    }

    public async ValueTask<GithubAccessTokenResponse?> GetAccessTokenByCodeAsync(string code)
    {
        string query = $"client_id={githubOptions.ClientId}&client_secret={githubOptions.ClientSecret}&code={code}";
        return await GetAccessTokenCoreAsync(query, code).ConfigureAwait(false);
    }

    private async ValueTask<GithubAccessTokenResponse?> GetAccessTokenCoreAsync(string query, string token)
    {
        using (HttpRequestMessage requestMessage = new(HttpMethod.Post, $"https://github.com/login/oauth/access_token?{query}"))
        {
            requestMessage.Headers.Accept.Add(new("application/json"));
            requestMessage.Headers.Authorization = new("token", token);

            using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage))
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return default;
                }

                return await responseMessage.Content.ReadFromJsonAsync<GithubAccessTokenResponse>();
            }
        }
    }
}