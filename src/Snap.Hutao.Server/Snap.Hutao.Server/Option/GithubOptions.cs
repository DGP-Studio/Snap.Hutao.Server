// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Option;

public sealed class GithubOptions
{
    public string ClientId { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    public byte[] StateEncryptKey { get; set; } = default!;

    public string Token { get; set; } = default!;
}