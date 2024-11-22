// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;

namespace Snap.Hutao.Server.Service.Distribution;

// Scoped
public sealed class DistributionService
{
    private readonly ILogger<DistributionService> logger;
    private readonly HttpClient httpClient;
    private readonly string cdnEndpoint;
    private readonly string cdnToken;

    public DistributionService(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetRequiredService<ILogger<DistributionService>>();
        httpClient = serviceProvider.GetRequiredService<HttpClient>();
        cdnEndpoint = serviceProvider.GetRequiredService<AppOptions>().CdnEndpoint;
        cdnToken = serviceProvider.GetRequiredService<AppOptions>().CdnToken;
    }

    public async ValueTask<HutaoPackageMirror?> GetAcceleratedMirrorAsync(string filename)
    {
        using (HttpRequestMessage req = new(HttpMethod.Get, string.Format(cdnEndpoint, filename)))
        {
            req.Headers.UserAgent.ParseAdd("Snap Hutao Server/1.0");
            req.Headers.Authorization = new("Bearer", cdnToken);

            using (HttpResponseMessage resp = await httpClient.SendAsync(req))
            {
                Response<HutaoPackageMirror>? mirrorResp = await resp.Content.ReadFromJsonAsync<Response<HutaoPackageMirror>>();
                ArgumentNullException.ThrowIfNull(mirrorResp);
                if (mirrorResp is not { Code: ReturnCode.Success })
                {
                    logger.LogWarning("Failed to get accelerated mirror for {Filename}, raw message: {Message}", filename, mirrorResp.Message);
                    return default;
                }

                return mirrorResp.Data;
            }
        }
    }
}