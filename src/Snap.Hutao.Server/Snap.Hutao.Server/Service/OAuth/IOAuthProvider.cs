// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.OAuth;

public interface IOAuthProvider
{
    Task<string> RequestAuthUrlAsync(string state);

    // Internal use
    Task<bool> RefreshTokenAsync(OAuthBindIdentity identity);
}