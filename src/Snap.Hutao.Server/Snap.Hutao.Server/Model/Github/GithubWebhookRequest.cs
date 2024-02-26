// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Github;

public sealed class GithubWebhookRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = default!;

    [JsonPropertyName("workflow_run")]
    public WorkflowRun? WorkflowRun { get; set; }

    [JsonPropertyName("release")]
    public Release? Release { get; set; }
}