// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Github;

[SuppressMessage("", "SA1201")]
public enum GithubWebhookEvent
{
    WorkflowRun,
    Release,
}