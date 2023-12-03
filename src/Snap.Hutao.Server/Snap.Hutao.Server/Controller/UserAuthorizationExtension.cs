// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Service.Authorization;

namespace Snap.Hutao.Server.Controller;

internal static class UserAuthorizationExtension
{
    public static int GetUserId(this ControllerBase controller)
    {
        return controller.HttpContext.GetUserId();
    }

    public static int GetUserId(this ActionExecutingContext context)
    {
        return context.HttpContext.GetUserId();
    }

    public static int GetUserId(this HttpContext context)
    {
        string value = context.User.Claims.Single(c => c.Type == PassportClaimTypes.UserId).Value;
        return int.Parse(value);
    }

    public static bool TryGetUserId(this ControllerBase controller, out int userId)
    {
        return controller.HttpContext.TryGetUserId(out userId);
    }

    public static bool TryGetUserId(this ActionExecutingContext context, out int userId)
    {
        return context.HttpContext.TryGetUserId(out userId);
    }

    public static bool TryGetUserId(this HttpContext context, out int userId)
    {
        string? value = context.User.Claims.SingleOrDefault(c => c.Type == PassportClaimTypes.UserId)?.Value;

        if (string.IsNullOrEmpty(value))
        {
            userId = 0;
            return false;
        }

        return int.TryParse(value, out userId);
    }

    public static async ValueTask<HutaoUser?> GetUserAsync(this ControllerBase controller, DbSet<HutaoUser> users)
    {
        if (controller.TryGetUserId(out int userId))
        {
            return await users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId).ConfigureAwait(false);
        }

        return default;
    }
}