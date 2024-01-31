// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Response;

public class Response
{
    public Response(ReturnCode code, string message)
    {
        Code = code;
        Message = message;
    }

    [JsonPropertyName("retcode")]
    public ReturnCode Code { get; set; }

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

    public static Response FailResponse(ReturnCode code, string msg, string key)
    {
        return new(code, msg) { LocalizationKey = key };
    }
}

[SuppressMessage("", "SA1402")]
public class Response<T> : Response
{
    public Response(ReturnCode code, string message, T data)
        : base(code, message)
    {
        Data = data;
    }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static IActionResult Success(string msg, T data)
    {
        return new JsonResult(new Response<T>(ReturnCode.Success, msg, data));
    }

    public static IActionResult Success(string msg, string key, T data)
    {
        return new JsonResult(new Response<T>(ReturnCode.Success, msg, data) { LocalizationKey = key });
    }
}