// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 请求统计
/// </summary>
[Table("request_statistics")]
public class RequestStatistics
{
    /// <summary>
    /// 构造一个新的请求统计
    /// </summary>
    /// <param name="context">请求上下文</param>
    public RequestStatistics(ActionExecutingContext context)
    {
        HttpRequest request = context.HttpContext.Request;

        UserAgent = request.Headers.UserAgent;
        Path = request.Path;
        Count = 0;
    }

    /// <summary>
    /// UA头
    /// </summary>
    public string UserAgent { get; set; } = default!;

    /// <summary>
    /// 请求路径
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// 请求次数
    /// </summary>
    public long Count { get; set; }
}