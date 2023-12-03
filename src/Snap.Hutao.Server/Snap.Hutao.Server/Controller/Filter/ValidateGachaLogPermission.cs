// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Controller.Filter;

public sealed class ValidateGachaLogPermission : IAsyncActionFilter
{
    private readonly AppDbContext appDbContext;

    public ValidateGachaLogPermission(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.TryGetUserId(out int userId))
        {
            context.Result = Response.Fail(ReturnCode.LoginFail, "请先登录或注册胡桃账号", ServerKeys.ServerPassportLoginRequired);
            return;
        }

        if (!await CanUseGachaLogServiceAsync(userId).ConfigureAwait(false))
        {
            context.Result = Response.Fail(ReturnCode.GachaLogServiceNotAllowed, "未开通祈愿记录上传服务或已到期", ServerKeys.ServerGachaLogServiceInsufficientTime);
            return;
        }

        // Execute next filter.
        ActionExecutedContext result = await next().ConfigureAwait(false);
    }

    private async Task<bool> CanUseGachaLogServiceAsync(int userId)
    {
        HutaoUser? user = await appDbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId)
            .ConfigureAwait(false);

        if (user is null)
        {
            return false;
        }

        return user.IsLicensedDeveloper || user.GachaLogExpireAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}