﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Controller.Filter;

/// <summary>
/// 请求统计筛选器
/// </summary>
public sealed class CountRequests : IAsyncActionFilter
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 构造一个新的请求统计筛选器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    public CountRequests(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string path = context.HttpContext.Request.Path;
        string? userAgent = context.HttpContext.Request.Headers.UserAgent;

        RequestStatistics? statistics = await appDbContext.RequestStatistics
            .Where(statistics => statistics.Path == path && statistics.UserAgent == userAgent)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);

        if (statistics == null)
        {
            statistics = RequestStatistics.CreateFromContext(context);
            appDbContext.RequestStatistics.Add(statistics);
        }

        statistics.Count++;
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        // Execute next filter.
        ActionExecutedContext result = await next().ConfigureAwait(false);
    }
}