// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class TokenResponse
{
    public string AccessToken { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;

    public int ExpiresIn { get; set; }
}