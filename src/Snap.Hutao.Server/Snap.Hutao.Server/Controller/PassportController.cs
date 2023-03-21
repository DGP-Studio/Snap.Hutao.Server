// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Authorization;
using System.Text;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 通行证控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class PassportController : ControllerBase
{
    private const string RandomRange = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    private readonly PassportService passportService;
    private readonly MailService mailService;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的通行证控制器
    /// </summary>
    /// <param name="passportService">通行证服务</param>
    /// <param name="mailService">邮件服务</param>
    /// <param name="memoryCache">内存缓存</param>
    public PassportController(PassportService passportService, MailService mailService, IMemoryCache memoryCache)
    {
        this.passportService = passportService;
        this.mailService = mailService;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 获取注册验证码
    /// </summary>
    /// <param name="userName">加密的用户名</param>
    /// <returns>响应</returns>
    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyAsync(EncryptedUserName userName)
    {
        string code = GetRandomStringWithChars();
        memoryCache.Set($"UserRegisterVerifyCode:{userName.UserName}", code, TimeSpan.FromMinutes(5));
        string decrypted = passportService.Decrypt(userName.UserName);
        await mailService.SendRegistrationVerifyCodeAsync(decrypted, code).ConfigureAwait(false);
        return Model.Response.Response.Success("请求验证码成功");
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="registration">加密的用户名密码</param>
    /// <returns>响应</returns>
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync(EncryptedRegistration registration)
    {
        string code = passportService.Decrypt(registration.VerifyCode);
        if (memoryCache.TryGetValue($"UserRegisterVerifyCode:{registration.UserName}", out string? storedCode))
        {
            if (storedCode == code)
            {
                Passport passport = new()
                {
                    UserName = passportService.Decrypt(registration.UserName),
                    Password = passportService.Decrypt(registration.Password),
                };

                PassportResult result = await passportService.RegisterAsync(passport).ConfigureAwait(false);
                if (result.Success)
                {
                    return Response<string>.Success("注册成功", result.Token);
                }
                else
                {
                    return Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message);
                }
            }
        }

        return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败");
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="encrypted">加密的用户名密码</param>
    /// <returns>响应</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync(EncryptedPassport encrypted)
    {
        Passport passport = new()
        {
            UserName = passportService.Decrypt(encrypted.UserName),
            Password = passportService.Decrypt(encrypted.Password),
        };

        PassportResult result = await passportService.LoginAsync(passport).ConfigureAwait(false);
        if (result.Success)
        {
            return Response<string>.Success("登录成功", result.Token);
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message);
        }
    }

    private static string GetRandomStringWithChars()
    {
        StringBuilder sb = new(6);

        for (int i = 0; i < 8; i++)
        {
            int pos = Random.Shared.Next(0, RandomRange.Length);
            sb.Append(RandomRange[pos]);
        }

        return sb.ToString();
    }
}