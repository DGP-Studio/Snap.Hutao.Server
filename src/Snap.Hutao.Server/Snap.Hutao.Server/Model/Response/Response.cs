// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// 成功
    /// </summary>
    /// <param name="msg">消息</param>
    /// <returns>操作结果</returns>
    public static IActionResult Success(string msg)
    {
        return new JsonResult(new Response(ReturnCode.Success, msg));
    }

    /// <summary>
    /// 任意数据
    /// </summary>
    /// <param name="code">返回代码</param>
    /// <param name="msg">消息</param>
    /// <returns>操作结果</returns>
    public static IActionResult Fail(ReturnCode code, string msg)
    {
        return new JsonResult(new Response(code, msg));
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

    /// <summary>
    /// 成功
    /// </summary>
    /// <param name="msg">消息</param>
    /// <param name="data">返回的数据</param>
    /// <returns>操作结果</returns>
    public static IActionResult Success(string msg, T data)
    {
        return new JsonResult(new Response<T>(ReturnCode.Success, msg, data));
    }
}
