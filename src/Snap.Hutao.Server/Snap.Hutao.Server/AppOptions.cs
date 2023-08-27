// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server;

public sealed class AppOptions
{
    public string Redis { get; set; } = default!;

    public string Afdian { get; set; } = default!;

    public string Jwt { get; set; } = default!;

    public SymmetricSecurityKey JwtSecurityKey
    {
        get => new(Encoding.UTF8.GetBytes(Jwt));
    }

    public string ReCaptchaKey { get; set; } = default!;

    public string RSAPrivateKey { get; set; } = default!;
}