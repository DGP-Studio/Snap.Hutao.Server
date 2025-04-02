// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Option;

namespace Snap.Hutao.Server.Service.StaticResource;

// Scoped
public sealed class StaticResourceService
{
    private readonly ILogger<StaticResourceService> logger;
    private readonly HttpClient httpClient;
    private readonly string cdnImgEndpoint;
    private readonly string cdnToken;

    public StaticResourceService(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetRequiredService<ILogger<StaticResourceService>>();
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
        cdnImgEndpoint = serviceProvider.GetRequiredService<AppOptions>().CdnImgEndpoint;
        cdnToken = serviceProvider.GetRequiredService<AppOptions>().CdnToken;
    }

    public async ValueTask<ImageToken?> GetAcceleratedImageTokenAsync(string username)
    {
        using (HttpRequestMessage req = new(HttpMethod.Post, cdnImgEndpoint))
        {
            req.Headers.UserAgent.ParseAdd("Snap Hutao Server/1.0");
            req.Headers.Authorization = new("Bearer", cdnToken);
            req.Content = new StringContent(username);

            using (HttpResponseMessage resp = await httpClient.SendAsync(req))
            {
                ImageToken? token = await resp.Content.ReadFromJsonAsync<ImageToken>();
                ArgumentNullException.ThrowIfNull(token);

                return token;
            }
        }
    }
}