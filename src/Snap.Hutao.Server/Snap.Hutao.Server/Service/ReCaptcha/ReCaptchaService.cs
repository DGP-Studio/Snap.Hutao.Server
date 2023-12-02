// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.ReCaptcha;
using Snap.Hutao.Server.Option;

namespace Snap.Hutao.Server.Service.ReCaptcha;

// Transient
public sealed class ReCaptchaService
{
    private readonly HttpClient httpClient;
    private readonly AppOptions appOptions;

    public ReCaptchaService(HttpClient httpClient, AppOptions appOptions)
    {
        this.httpClient = httpClient;
        this.appOptions = appOptions;
    }

    public async Task<ReCaptchaResponse?> VerifyAsync(string token)
    {
        string url = $"https://www.google.com/recaptcha/api/siteverify?secret={appOptions.ReCaptchaKey}&response={token}";
        HttpResponseMessage message = await httpClient.PostAsync(url, null).ConfigureAwait(false);
        return await message.Content.ReadFromJsonAsync<ReCaptchaResponse>().ConfigureAwait(false);
    }
}
