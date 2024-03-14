// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Github;

public sealed class GithubWebhookResult
{
    public GithubWebhookEvent Event { get; set; }

    public string MarkdownBody { get; set; } = default!;

    public string Filename { get; set; } = default!;
}