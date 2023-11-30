// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Controller.Filter;

public sealed class ValidateMaintainPermission : IAsyncActionFilter
{
    private readonly AppDbContext appDbContext;

    public ValidateMaintainPermission(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.TryGetUserId(out int userId))
        {
            if (await IsMaintainerAsync(userId).ConfigureAwait(false))
            {
                // Execute next filter.
                ActionExecutedContext result = await next().ConfigureAwait(false);
            }
        }

        context.Result = Response.Fail(ReturnCode.ServiceKeyInvalid, "只有官方人员可以这么做");
    }

    private async Task<bool> IsMaintainerAsync(int userId)
    {
        HutaoUser? user = await appDbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId)
            .ConfigureAwait(false);

        return user != null && user.IsMaintainer;
    }
}
