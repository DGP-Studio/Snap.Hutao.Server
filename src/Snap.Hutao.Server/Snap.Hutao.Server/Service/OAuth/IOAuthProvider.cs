// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.OAuth;

public interface IOAuthProvider
{
    Task<string> RequestAuthUrlAsync(string state);

    // Internal use
    // TODO: add a job to refresh refresh token
    Task<bool> RefreshTokenAsync(OAuthBindIdentity identity);
}