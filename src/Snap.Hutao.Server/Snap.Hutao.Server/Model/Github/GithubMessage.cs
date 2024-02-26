// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class GithubMessage
{
    public GithubMessageType Type { get; set; }

    public string MarkdownBody { get; set; } = default!;

    public string Filename { get; set; } = default!;

    public Stream Stream { get; set; } = default!;
}

[SuppressMessage("", "SA1201")]
public enum GithubMessageType
{
    WorkflowRun,
    Release,
}