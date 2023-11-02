// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Controller.Helper;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Authorization;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 通行证控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(GroupName = "Passport")]
public class PassportController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly PassportService passportService;
    private readonly MailService mailService;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// 构造一个新的通行证控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="passportService">通行证服务</param>
    /// <param name="mailService">邮件服务</param>
    /// <param name="memoryCache">内存缓存</param>
    public PassportController(AppDbContext appDbContext, PassportService passportService, MailService mailService, IMemoryCache memoryCache)
    {
        this.appDbContext = appDbContext;
        this.passportService = passportService;
        this.mailService = mailService;
        this.memoryCache = memoryCache;
    }

    /// <summary>
    /// 获取注册验证码
    /// </summary>
    /// <param name="request">加密的验证请求</param>
    /// <returns>响应</returns>
    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyAsync([FromBody] PassportRequest request)
    {
        string userName = passportService.Decrypt(request.UserName);
        string normalizedUserName = userName.ToUpperInvariant();

        if (memoryCache.TryGetValue($"VerifyCodeFor:{normalizedUserName}", out object? _))
        {
            return Model.Response.Response.Fail(ReturnCode.VerifyCodeTooFrequently, "请求过快，请 1 分钟后再试", "ServerPassportVerifyTooFrequent");
        }
        else
        {
            string code = RandomHelper.GetRandomStringWithChars(8);
            memoryCache.Set($"VerifyCodeFor:{normalizedUserName}", code, TimeSpan.FromMinutes(1));

            if (request.IsResetPassword)
            {
                await mailService.SendResetPasswordVerifyCodeAsync(userName, code).ConfigureAwait(false);
            }
            else if (request.IsCancelRegistration)
            {
                await mailService.SendCancelRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
            }
            else
            {
                await mailService.SendRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
            }

            return Model.Response.Response.Success("请求验证码成功", "ServerPassportVerifyRequestSuccess");
        }
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="request">加密的用户名密码</param>
    /// <returns>响应</returns>
    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] PassportRequest request)
    {
        string code = passportService.Decrypt(request.VerifyCode);
        string userName = passportService.Decrypt(request.UserName);
        string normalizedUserName = userName.ToUpperInvariant();

        if (memoryCache.TryGetValue($"VerifyCodeFor:{normalizedUserName}", out string? storedCode))
        {
            if (storedCode == code)
            {
                Passport passport = new()
                {
                    UserName = userName,
                    Password = passportService.Decrypt(request.Password),
                };

                PassportResult result = await passportService.RegisterAsync(passport).ConfigureAwait(false);
                return result.Success
                    ? Response<string>.Success("注册成功", result.Token)
                    : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message);
            }
        }

        return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", "ServerPassportVerifyFailed");
    }

    [Authorize]
    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelAsync([FromBody] PassportRequest request)
    {
        if (await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false) is { } user)
        {
            Passport passport = new()
            {
                UserName = user.UserName!,
                Password = passportService.Decrypt(request.Password),
            };

            PassportResult result = await passportService.CancelAsync(passport).ConfigureAwait(false);

            return result.Success
                ? Model.Response.Response.Success(result.Message)
                : Model.Response.Response.Fail(ReturnCode.CancelFail, result.Message);
        }

        return Model.Response.Response.Fail(ReturnCode.CancelFail, "用户名或密码错误", "ServerPassportUsernameOrPassportIncorrect");
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="request">加密的用户名密码</param>
    /// <returns>响应</returns>
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] PassportRequest request)
    {
        string code = passportService.Decrypt(request.VerifyCode);
        string userName = passportService.Decrypt(request.UserName);
        string normalizedUserName = userName.ToUpperInvariant();

        if (memoryCache.TryGetValue($"VerifyCodeFor:{normalizedUserName}", out string? storedCode))
        {
            if (storedCode == code)
            {
                Passport passport = new()
                {
                    UserName = userName,
                    Password = passportService.Decrypt(request.Password),
                };

                PassportResult result = await passportService.ResetPasswordAsync(passport).ConfigureAwait(false);
                if (result.Success)
                {
                    return Response<string>.Success("密码设置成功", result.Token);
                }
                else
                {
                    return Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
                }
            }
        }

        return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", "ServerPassportVerifyFailed");
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="request">加密的用户名密码</param>
    /// <returns>响应</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] PassportRequest request)
    {
        Passport passport = new()
        {
            UserName = passportService.Decrypt(request.UserName),
            Password = passportService.Decrypt(request.Password),
        };

        PassportResult result = await passportService.LoginAsync(passport).ConfigureAwait(false);
        if (result.Success)
        {
            return Response<string>.Success("登录成功", result.Token);
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.LoginFail, result.Message, result.LocalizationKey!);
        }
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [Authorize]
    [HttpGet("UserInfo")]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);

        if (user is not null)
        {
            return Response<UserInfo>.Success("获取用户信息成功", new()
            {
                NormalizedUserName = user.NormalizedUserName ?? user.UserName,
                IsLicensedDeveloper = user.IsLicensedDeveloper,
                IsMaintainer = user.IsMaintainer,
                GachaLogExpireAt = DateTimeOffset.FromUnixTimeSeconds(user.GachaLogExpireAt),
            });
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.UserNameNotExists, "用户不存在", "ServerPassportUserInfoNotExist");
        }
    }
}