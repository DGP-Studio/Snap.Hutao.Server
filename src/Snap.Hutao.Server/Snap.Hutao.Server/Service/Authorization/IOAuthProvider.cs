// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.Authorization;

public interface IOAuthProvider
{
    string ProviderName { get; }

    Task<OAuthUserInfo?> GetUserInfoAsync(string accessToken);

    Task<OAuthTokenResponse?> ExchangeCodeForTokenAsync(string code);

    Task<OAuthTokenResponse?> RefreshTokenAsync(string refreshToken);
}