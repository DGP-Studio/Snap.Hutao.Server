// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization.Policy;
using Snap.Hutao.Server.Controller;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Service.Authorization;

// Singleton
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

        await context.Response.WriteAsJsonAsync(Response.Fail(ReturnCode.LoginFail, "请先登录或注册胡桃账号", ServerKeys.ServerPassportLoginRequired)).ConfigureAwait(false);
    }
}