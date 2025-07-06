// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class OAuthTokenResponse
{
    public string AccessToken { get; set; } = default!;

    public string? RefreshToken { get; set; }

    public int ExpiresIn { get; set; }
}