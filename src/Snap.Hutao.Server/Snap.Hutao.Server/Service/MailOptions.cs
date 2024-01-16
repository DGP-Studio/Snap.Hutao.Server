// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service;

public sealed class MailOptions
{
    public required string Address { get; set; }

    public required string Subject { get; set; }

    public required string Title { get; set; }

    public required string Footer { get; set; }

    public required string RawContent { get; set; }
}