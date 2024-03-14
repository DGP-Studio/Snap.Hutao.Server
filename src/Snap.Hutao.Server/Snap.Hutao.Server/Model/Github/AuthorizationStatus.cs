// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class AuthorizationStatus
{
    public bool IsAuthorized { get; set; }

    public string? AccessToken { get; set; }
}