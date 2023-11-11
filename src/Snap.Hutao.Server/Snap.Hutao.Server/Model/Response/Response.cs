// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Snap.Hutao.Server.Model.Response;

/// <summary>
/// 响应
/// </summary>
public class Response
{
    /// <summary>
    /// 构造一个新的响应
    /// </summary>
    /// <param name="code">响应代码</param>
    /// <param name="message">消息</param>
    public Response(ReturnCode code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// 返回代码
    /// </summary>
    [JsonPropertyName("retcode")]
    public ReturnCode Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("l10nKey")]
    public string? LocalizationKey { get; set; }

    public static IActionResult Success(string msg)
    {
        return new JsonResult(new Response(ReturnCode.Success, msg));
    }

    public static IActionResult Success(string msg, string key)
    {
        return new JsonResult(new Response(ReturnCode.Success, msg) { LocalizationKey = key });
    }

    public static IActionResult Fail(ReturnCode code, string msg)
    {
        return new JsonResult(new Response(code, msg));
    }

    public static IActionResult Fail(ReturnCode code, string msg, string key)
    {
        return new JsonResult(new Response(code, msg) { LocalizationKey = key });
    }
}

/// <summary>
/// 带有数据的响应
/// </summary>
/// <typeparam name="T">T</typeparam>
[SuppressMessage("", "SA1402")]
public class Response<T> : Response
{
    /// <summary>
    /// 构造一个新的响应
    /// </summary>
    /// <param name="code">响应代码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    public Response(ReturnCode code, string message, T data)
        : base(code, message)
    {
        Data = data;
    }

    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static IActionResult Success(string msg, T data)
    {
        return new JsonResult(new Response<T>(ReturnCode.Success, msg, data));
    }

    public static IActionResult Success(string msg, T data, string key)
    {
        return new JsonResult(new Response<T>(ReturnCode.Success, msg, data) { LocalizationKey = key });
    }
}
