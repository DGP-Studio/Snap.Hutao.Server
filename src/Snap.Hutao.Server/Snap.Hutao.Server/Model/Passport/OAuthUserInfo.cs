// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class OAuthUserInfo
{
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? AvatarUrl { get; set; }
}