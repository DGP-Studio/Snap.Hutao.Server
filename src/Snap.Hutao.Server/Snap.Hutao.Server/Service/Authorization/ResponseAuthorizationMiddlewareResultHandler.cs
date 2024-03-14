// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization.Policy;
using Snap.Hutao.Server.Controller;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Service.Authorization;

// Singleton
public class ResponseAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    /// <inheritdoc/>
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            return defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        return ForbidAsync(context);
    }

    private static async Task ForbidAsync(HttpContext context)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
        }

        Response response = Response.FailResponse(ReturnCode.LoginFail, "请先登录或注册胡桃账号", ServerKeys.ServerPassportLoginRequired);
        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }
}