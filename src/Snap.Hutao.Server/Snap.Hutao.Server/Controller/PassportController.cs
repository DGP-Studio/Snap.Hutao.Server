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
// TODO: add v2 api versioning which returns full token response instead of just access token
// DO NOT remove old api versioning due to client compatibility
// add endpoints with "v2" prefix OR add a new controller (e.g. PassportV2Controller) as v2
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
        string key = request switch
        {
            { IsResetPassword: true } => "ResetPassword",
            { IsResetUsername: true } => "ResetUsername",
            { IsResetUsernameNew: true } => "ResetUsernameNew",
            { IsCancelRegistration: true } => "CancelRegistration",
            _ => "Registration",
        };

        if (passportVerificationService.TryGetNonExpiredVerifyCode(normalizedUserName, key, out _))
        {
            return Model.Response.Response.Fail(ReturnCode.VerifyCodeTooFrequently, "请求过快，请 1 分钟后再试", ServerKeys.ServerPassportVerifyTooFrequent);
        }

        bool userExists = appDbContext.Users.Any(u => u.NormalizedUserName == normalizedUserName);
        string code = passportVerificationService.GenerateVerifyCode(normalizedUserName, key);

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

        if (!this.passportVerificationService.TryValidateVerifyCode(normalizedUserName, "Registration", code))
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
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token.AccessToken)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelRegistrationAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, "CancelRegistration", code))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        Passport passport = new()
        {
            UserName = userName,
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

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, "ResetPassword", code))
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
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token.AccessToken)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("ResetUsername")]
    public async Task<IActionResult> ResetUsernameAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);
        string newNormalizedUserName = passportService.DecryptNewNormalizedUserNameAndNewVerifyCode(request, out string newUserName, out string newCode);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, "ResetUsername", code) || !passportVerificationService.TryValidateVerifyCode(newNormalizedUserName, "ResetUsernameNew", newCode))
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
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token.AccessToken)
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
            ? Response<string>.Success(result.Message, result.LocalizationKey!, result.Token.AccessToken)
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

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidRequestBody, "刷新令牌不能为空", ServerKeys.ServerPassportRefreshTokenEmpty);
        }

        TokenResponse? tokenResponse = await passportService.RefreshTokenAsync(request.RefreshToken);
        if (tokenResponse is null)
        {
            return Model.Response.Response.Fail(ReturnCode.LoginFail, "刷新令牌无效或已过期", ServerKeys.ServerPassportRefreshTokenInvalid);
        }

        return Response<TokenResponse>.Success("令牌刷新成功", tokenResponse);
    }

    [Authorize]
    [HttpPost("RevokeToken")]
    public async Task<IActionResult> RevokeTokenAsync([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidRequestBody, "刷新令牌不能为空", ServerKeys.ServerPassportRefreshTokenEmpty);
        }

        return await passportService.RevokeRefreshTokenAsync(request.RefreshToken).ConfigureAwait(false)
            ? Model.Response.Response.Success("令牌撤销成功", ServerKeys.ServerPassportTokenRevokeSuccess)
            : Model.Response.Response.Fail(ReturnCode.RefreshTokenDbException, "令牌撤销失败", ServerKeys.ServerPassportTokenRevokeFailed);
    }

    [Authorize]
    [HttpPost("RevokeAllTokens")]
    public async Task<IActionResult> RevokeAllTokensAsync()
    {
        int userId = this.GetUserId();
        await passportService.RevokeAllUserTokensAsync(userId);
        return Model.Response.Response.Success("所有令牌已撤销", ServerKeys.ServerPassportTokenRevokeSuccess);
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
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, "ResetPassword");
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
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, "ResetUsername");
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
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, "CancelRegistration");
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestNotCurrentUser);
                }

                await mailService.SendCancelRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            // 注册账号，用户名已存在
            if (userExists)
            {
                passportVerificationService.RemoveVerifyCode(normalizedUserName, "Registration");
                passportVerificationService.RemoveVerifyCode(normalizedUserName, "ResetUsernameNew");
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