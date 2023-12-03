// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service;

public sealed class MailOptions
{
    public string Address { get; set; } = default!;

    public string Subject { get; set; } = default!;

    public List<(string Tag, string? Attribute, string Content)> Bodys { get; set; } = [];
}