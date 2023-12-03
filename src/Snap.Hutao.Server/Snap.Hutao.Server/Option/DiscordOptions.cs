// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Option;

public sealed class DiscordOptions
{
    public ulong AllowedGuildId { get; set; }

    public List<ulong> OwnerIds { get; set; } = default!;

    public string Token { get; set; } = default!;

    public KnownDiscordChannelOptions KnownChannels { get; set; } = default!;
}