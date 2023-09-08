// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Service.Authorization;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 控制器扩展
/// </summary>
internal static class ControllerBaseExtension
{
    /// <summary>
    /// 获取用户Id
    /// </summary>
    /// <param name="controller">控制器</param>
    /// <returns>用户Id</returns>
    public static int GetUserId(this ControllerBase controller)
    {
        string value = controller.HttpContext.User.Claims.Single(c => c.Type == PassportClaimTypes.UserId).Value;
        return int.Parse(value);
    }

    public static async ValueTask<HutaoUser?> GetUserAsync(this ControllerBase controller, DbSet<HutaoUser> users)
    {
        int userId = controller.GetUserId();
        return await users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId).ConfigureAwait(false);
    }
}