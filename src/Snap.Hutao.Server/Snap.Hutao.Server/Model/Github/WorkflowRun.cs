// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class WorkflowRun
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("pull_requests")]
    public List<object> PullRequests { get; set; } = default!;

    [JsonPropertyName("artifacts_url")]
    public string ArtifactsUrl { get; set; } = default!;

    [JsonPropertyName("head_commit")]
    public HeadCommit HeadCommit { get; set; } = default!;
}