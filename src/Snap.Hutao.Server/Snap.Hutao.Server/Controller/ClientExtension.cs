// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Controller;

internal static class ClientExtension
{
    private const string SnapHutaoClientHeaderPrefix = "Snap Hutao/";

    public static bool TryGetClientVersion(this ControllerBase controller, [NotNullWhen(true)] out Version? version)
    {
        return controller.HttpContext.TryGetClientVersion(out version);
    }

    public static bool TryGetClientVersion(this ActionExecutingContext context, [NotNullWhen(true)] out Version? version)
    {
        return context.HttpContext.TryGetClientVersion(out version);
    }

    public static bool TryGetClientVersion(this HttpContext context, [NotNullWhen(true)] out Version? version)
    {
        string? userAgent = context.Request.Headers.UserAgent;

        if (string.IsNullOrEmpty(userAgent) || !userAgent.StartsWith(SnapHutaoClientHeaderPrefix))
        {
            version = default;
            return false;
        }

        return Version.TryParse(userAgent[SnapHutaoClientHeaderPrefix.Length..], out version);
    }
}