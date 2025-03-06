// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Primitives;
using Sentry;

namespace Snap.Hutao.Server.Service.Sentry;

public sealed class SentryUserFactory : ISentryUserFactory
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public SentryUserFactory(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public SentryUser? Create()
    {
        return httpContextAccessor.HttpContext is { } context ? Create(context) : default;
    }

    private static SentryUser? Create(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("x-hutao-device-id", out StringValues deviceIds))
        {
            return default;
        }

        string deviceId = deviceIds.ToString();

        return new()
        {
            Id = deviceId,
        };
    }
}