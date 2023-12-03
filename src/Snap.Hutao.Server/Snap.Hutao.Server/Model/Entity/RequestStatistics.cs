// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 请求统计
/// </summary>
[Table("request_statistics")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public class RequestStatistics
{
    /// <summary>
    /// EF用
    /// </summary>
    public RequestStatistics()
    {
    }

    public RequestStatistics(ActionExecutingContext context)
    {
        HttpRequest request = context.HttpContext.Request;

        UserAgent = request.Headers.UserAgent.ToString();
        Path = request.Path;
        Count = 0;
    }

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

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

    public static RequestStatistics CreateFromContext(ActionExecutingContext context)
    {
        HttpRequest request = context.HttpContext.Request;
        return new()
        {
            UserAgent = request.Headers.UserAgent.ToString(),
            Path = request.Path,
            Count = 0,
        };
    }
}