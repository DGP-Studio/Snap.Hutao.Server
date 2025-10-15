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
[Route("Passport/v2")]
[ApiExplorerSettings(GroupName = "Passport")]
public class PassportV2Controller : ControllerBase
{
    private readonly PassportVerificationService passportVerificationService;
    private readonly AppDbContext appDbContext;
    private readonly PassportService passportService;
    private readonly MailService mailService;

    public PassportV2Controller(IServiceProvider serviceProvider)
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
            { IsResetPassword: true } => PassportVerificationService.ResetPassword,
            { IsResetUserName: true } => PassportVerificationService.ResetUserName,
            { IsResetUserNameNew: true } => PassportVerificationService.ResetUserNameNew,
            { IsCancelRegistration: true } => PassportVerificationService.CancelRegistration,
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
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!this.passportVerificationService.TryValidateVerifyCode(normalizedUserName, PassportVerificationService.Registration, code))
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

        PassportResult result = await passportService.RegisterAsync(passport, deviceInfo).ConfigureAwait(false);
        return result.Success
            ? Response<TokenResponse>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelRegistrationAsync([FromBody] PassportRequest request)
    {
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, PassportVerificationService.CancelRegistration, code))
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
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, PassportVerificationService.ResetPassword, code))
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

        PassportResult result = await passportService.ResetPasswordAsync(passport, deviceInfo).ConfigureAwait(false);

        return result.Success
            ? Response<TokenResponse>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("ResetUsername")]
    public async Task<IActionResult> ResetUsernameAsync([FromBody] PassportRequest request)
    {
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        string normalizedUserName = passportService.DecryptNormalizedUserNameAndVerifyCode(request, out string userName, out string code);
        string newNormalizedUserName = passportService.DecryptNewNormalizedUserNameAndNewVerifyCode(request, out string newUserName, out string newCode);

        if (!passportVerificationService.TryValidateVerifyCode(normalizedUserName, PassportVerificationService.ResetUserName, code) ||
            !passportVerificationService.TryValidateVerifyCode(newNormalizedUserName, PassportVerificationService.ResetUserNameNew, newCode))
        {
            return Model.Response.Response.Fail(ReturnCode.RegisterFail, "验证失败", ServerKeys.ServerPassportVerifyFailed);
        }

        Passport passport = new()
        {
            UserName = userName,
            NewUserName = newUserName,
        };

        PassportResult result = await passportService.ResetUsernameAsync(passport, deviceInfo).ConfigureAwait(false);

        return result.Success
            ? Response<TokenResponse>.Success(result.Message, result.LocalizationKey!, result.Token)
            : Model.Response.Response.Fail(ReturnCode.RegisterFail, result.Message, result.LocalizationKey!);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] PassportRequest request)
    {
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        Passport passport = new()
        {
            UserName = passportService.Decrypt(request.UserName),
            Password = passportService.Decrypt(request.Password),
        };

        PassportResult result = await passportService.LoginAsync(passport, deviceInfo).ConfigureAwait(false);

        return result.Success
            ? Response<TokenResponse>.Success(result.Message, result.LocalizationKey!, result.Token)
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

    [Authorize]
    [HttpGet("LoggedInDevices")]
    public async Task<IActionResult> GetLoggedInDevicesAsync()
    {
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        int userId = this.GetUserId();
        List<LoggedInDeviceInfo> devices = await passportService.GetLoggedInDevicesAsync(userId, deviceInfo).ConfigureAwait(false);
        return Response<List<LoggedInDeviceInfo>>.Success("获取已登录设备成功", devices);
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidRequestBody, "刷新令牌不能为空", ServerKeys.ServerPassportRefreshTokenEmpty);
        }

        TokenResponse? tokenResponse = await passportService.RefreshTokenAsync(passportService.Decrypt(request.RefreshToken));
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
        if (string.IsNullOrEmpty(request.DeviceId))
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidRequestBody, "设备 ID 不能为空", ServerKeys.ServerPassportDeviceIdEmpty);
        }

        return await passportService.RevokeRefreshTokenAsync(passportService.Decrypt(request.DeviceId)).ConfigureAwait(false)
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
            if (request.IsResetPassword)
            {
                if (!userExists)
                {
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.ResetPassword);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNotExisted);
                }

                await mailService.SendResetPasswordVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            if (request.IsResetUserName)
            {
                if (!userExists)
                {
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.ResetUserName);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNotExisted);
                }

                await mailService.SendResetUsernameVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            if (request.IsCancelRegistration)
            {
                HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);
                if (user is null || !string.Equals(normalizedUserName, user.NormalizedUserName, StringComparison.Ordinal))
                {
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.CancelRegistration);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestNotCurrentUser);
                }

                await mailService.SendCancelRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            if (userExists)
            {
                passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.Registration);
                passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.ResetUserNameNew);
                return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserAlreadyExisted);
            }

            if (request.IsResetUserNameNew)
            {
                await mailService.SendResetUsernameVerifyCodeAsync(userName, code).ConfigureAwait(false);
                return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
            }

            await mailService.SendRegistrationVerifyCodeAsync(userName, code).ConfigureAwait(false);
            return Model.Response.Response.Success("请求验证码成功", ServerKeys.ServerPassportVerifyRequestSuccess);
        }
        catch (ParseException)
        {
            return Model.Response.Response.Fail(ReturnCode.InvalidUserName, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNameInvalid);
        }
    }
}