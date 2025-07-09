// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.OAuth;

public sealed class OAuthBindState
{
    public OAuthBindState(string callbackUri)
    {
        CallbackUri = callbackUri;
    }

    public OAuthBindState(int userId, string callbackUri)
    {
        UserId = userId;
        CallbackUri = callbackUri;
    }

    public int UserId { get; set; } = -1;

    public string CallbackUri { get; set; }
}