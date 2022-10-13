// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Controller.Filter;

/// <summary>
/// 请求统计筛选器
/// </summary>
public class RequestFilter : IAsyncActionFilter
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 构造一个新的请求统计筛选器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    public RequestFilter(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string path = context.HttpContext.Request.Path;
        string ua = context.HttpContext.Request.Headers.UserAgent;

        using (await appDbContext.OperationLock.EnterAsync().ConfigureAwait(false))
        {
            RequestStatistics? statistics = appDbContext.RequestStatistics
                .Where(req => req.Path == path)
                .SingleOrDefault(req => req.UserAgent == ua);

            if (statistics == null)
            {
                statistics = new(context);
                appDbContext.RequestStatistics.Add(statistics);
            }

            statistics.Count++;
            appDbContext.SaveChanges();
        }

        // Execute next filter.
        ActionExecutedContext result = await next().ConfigureAwait(false);
    }
}
