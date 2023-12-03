// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Option;

internal static class AppOptionsExtension
{
    public static SymmetricSecurityKey GetJwtSecurityKey(this AppOptions options)
    {
        return new(Encoding.UTF8.GetBytes(options.JwtRaw));
    }
}