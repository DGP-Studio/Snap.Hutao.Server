// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization.Policy;

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 填充响应的权限验证中间件处理器
/// </summary>
public class ResponseAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    /// <inheritdoc/>
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            return next(context);
        }

        return ForbidAsync(context);
    }

    private static async Task ForbidAsync(HttpContext context)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
        }

        await context.Response.WriteAsJsonAsync(new Model.Response.Response(Model.Response.ReturnCode.LoginFail, "请先登录或注册胡桃账号")).ConfigureAwait(false);
    }
}