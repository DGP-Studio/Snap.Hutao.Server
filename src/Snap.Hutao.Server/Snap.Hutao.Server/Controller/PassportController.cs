// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using MailKit.Net.Smtp;
using MimeKit;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Authorization;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Passport")]
public class PassportController : ControllerBase
{
    private readonly PassportVerificationService passportVerificationService;
    private readonly AppDbContext appDbContext;
    private readonly PassportService passportService;
    private readonly MailService mailService;

    public PassportController(IServiceProvider serviceProvider)
    {
        passportVerificationService = serviceProvider.GetRequiredService<PassportVerificationService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        passportService = serviceProvider.GetRequiredService<PassportService>();
        mailService = serviceProvider.GetRequiredService<MailService>();
    }

    [HttpPost("Verify")]
    public async Task<IActionResult> RequestVerifyCodeAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserName(request, out string userName);

        if (passportVerificationService.TryGetNonExpiredVerifyCode(normalizedUserName, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.VerifyCodeTooFrequently, "请求过快，请 1 分钟后再试", ServerKeys.ServerPassportVerifyTooFrequent);
        }

        bool userExists = appDbContext.Users.Any(u => u.NormalizedUserName == normalizedUserName);
        string code = passportVerificationService.GenerateVerifyCodeForUserName(normalizedUserName);

        try
        {
            return await PrivateRequestVerifyCodeAsync(request, normalizedUserName, userName, userExists, code).ConfigureAwait(false);
        }
        catch (SmtpCommandException)
        {
            return Model.Response.Response.Success("请求验证码失败", ServerKeys.ServerPassportVerifyRequestFailed);
        }
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, code))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        Passport passport = new()
        {
            UserName = userName,
            Password = passportService.Decrypt(request.Password),
        };

        if (passport.Password.Length < 8)
        {
            return Model.Response.Response.Fail(ReturnCode.TooShortPassword, "密码长度不能小于 8 位", ServerKeys.ServerPassportPasswordTooShort);
        }

        PassportResult result = await passportService.RegisterAsync(passport).ConfigureAwait(false);
        return result.Success
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [Authorize]
    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelRegistrationAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, code))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        if (await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false) is not { } user)
        {
            return Model.Response.Response.Fail(ReturnCode.CancelFail, "用户名或密码错误", ServerKeys.ServerPassportUserNameOrPasswordIncorrect);
        }

        Passport passport = new()
        {
            UserName = user.UserName!,
            Password = passportService.Decrypt(request.Password),
        };

        PassportResult result = await passportService.CancelAsync(passport).ConfigureAwait(false);

        return result.Success
            ? Model.Response.Response.Success(result.Message, result.LocalizationKey!)
            : Model.Response.Response.Fail(ReturnCode.CancelFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, code))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        Passport passport = new()
        {
            UserName = userName,
            Password = passportService.Decrypt(request.Password),
        };

        if (passport.Password.Length < 8)
        {
            return Model.Response.Response.Fail(ReturnCode.TooShortPassword, "密码长度不能小于 8 位", ServerKeys.ServerPassportPasswordTooShort);
        }

        PassportResult result = await passportService.ResetPasswordAsync(passport).ConfigureAwait(false);

        return result.Success
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("ResetUsername")]
    public async Task<IActionResult> ResetUsernameAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);
        string newNormalizedUserName = passportService.DecryptNewNormalizedUserNameAndNewVerifyCode(request, out string newUserName, out string newCode);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, code) || !passportVerificationService.TryValidateVerifyCode(newNormalizedUserName, newCode))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        Passport passport = new()
        {
            UserName = userName,
            NewUserName = newUserName,
        };

        PassportResult result = await passportService.ResetUsernameAsync(passport).ConfigureAwait(false);

        return result.Success
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] PassportRequest request)
    {
        Passport passport = new()
        {
            UserName = passportService.Decrypt(request.UserName),
            Password = passportService.Decrypt(request.Password),
        };

        PassportResult result = await passportService.LoginAsync(passport).ConfigureAwait(false);

        return result.Success
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.LoginFail, result.Message, result.LocalizationKey!);
    }

    [Authorize]
    [HttpGet("UserInfo")]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);

        if (user is null)
        {
            return Model.Response.Response.Fail(ReturnCode.UserNameNotExists, "用户不存在", ServerKeys.ServerPassportUserInfoNotExist);
        }

        return Response<UserInfo>.Success("获取用户信息成功", new()
        {
            NormalizedUserName = user.NormalizedUserName ?? user.UserName,
            UserName = user.UserName,
            IsLicensedDeveloper = user.IsLicensedDeveloper,
            IsMaintainer = user.IsMaintainer,
            GachaLogExpireAt = DateTimeOffset.FromUnixTimeSeconds(user.GachaLogExpireAt),
            CdnExpireAt = DateTimeOffset.FromUnixTimeSeconds(user.CdnExpireAt),
        });
    }

    private async Task<IActionResult> PrivateRequestVerifyCodeAsync(PassportRequest request, string normalizedUserName, string userName, bool userExists, string code)
    {
        try
        {
            // 重置密码
            if (request.IsResetPassword)
            {
                if (!userExists)
                {
                    passportVerificationService.RemoveVerifyCodeForUserName(normalizedUserName);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNotExisted);
                }

                await mailService.SendResetPasswordVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            // 重置用户名
            if (request.IsResetUsername)
            {
                if (!userExists)
                {
                    passportVerificationService.RemoveVerifyCodeForUserName(normalizedUserName);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNotExisted);
                }

                await mailService.SendResetUsernameVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            // 注销账号
            if (request.IsCancelRegistration)
            {
                // 注销时特判，只能注销当前用户
                HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);
                if (user is null || !string.Equals(normalizedUserName, user.NormalizedUserName, StringComparison.Ordinal))
                {
                    passportVerificationService.RemoveVerifyCodeForUserName(normalizedUserName);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestNotCurrentUser);
                }

                await mailService.SendCancelRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            // 注册账号，用户名已存在
            if (userExists)
            {
                passportVerificationService.RemoveVerifyCodeForUserName(normalizedUserName);
                return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserAlreadyExisted);
            }

            if (request.IsResetUsernameNew)
            {
                await mailService.SendResetUsernameVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            // 默认注册
            await mailService.SendRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
            return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
        }
        catch (ParseException)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUserName, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNameInvalid);
        }
    }
}