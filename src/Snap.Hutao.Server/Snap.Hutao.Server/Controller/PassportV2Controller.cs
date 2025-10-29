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
using Swashbuckle.AspNetCore.Annotations;

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

    /// <summary>
    /// 申请邮箱验证码以完成注册、找回密码或账号安全操作。
    /// </summary>
    [SwaggerOperation(
        Summary = "申请邮箱验证码以完成注册、找回密码或账号安全操作。",
        Description = """
        请求体字段：
        - UserName：使用服务端提供的 RSA 公钥加密并进行 Base64 编码的邮箱账号。
        - IsResetPassword/IsResetUserName/IsResetUserNameNew/IsCancelRegistration：根据场景设置对应标记，其余保持 false。

        注意事项：
        - 同一邮箱同一类型验证码在 60 秒内只能申请一次，请在客户端进行节流处理。
        - 若需要重置用户名，请分别提交旧邮箱与新邮箱两次请求以获取成对验证码。
        """)]
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

    /// <summary>
    /// 使用邮箱验证码注册 Snap Hutao 账号并下发一组访问令牌。
    /// </summary>
    [SwaggerOperation(
        Summary = "使用邮箱验证码注册 Snap Hutao 账号并下发一组访问令牌。",
        Description = """
        请求体字段：
        - UserName：RSA 加密后 Base64 编码的邮箱账号。
        - Password：RSA 加密后 Base64 编码的新密码，至少 8 位。
        - VerifyCode：通过“申请验证码”接口获取的 6 位纯数字验证码。

        注意事项：
        - 首次注册会自动发放 3 天胡桃云使用权限，客户端需持久化 TokenResponse 中的刷新令牌。
        - 若验证码已失效或被重复使用，将返回 RegisterFail 错误码。
        """)]
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

    /// <summary>
    /// 注销现有账号并删除关联数据。
    /// </summary>
    [SwaggerOperation(
        Summary = "注销现有账号并删除关联数据。",
        Description = """
        请求体字段：
        - UserName：RSA 加密后 Base64 编码的邮箱账号。
        - Password：RSA 加密后 Base64 编码的当前登录密码。
        - VerifyCode：申请验证码时设置 IsCancelRegistration 标记后收到的验证码。

        注意事项：
        - 注销操作不可逆，一旦成功所有刷新令牌都会失效。
        - 验证码必须由当前登录用户申请，以避免误操作风险。
        """)]
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

    /// <summary>
    /// 通过邮箱验证码设置新的登录密码。
    /// </summary>
    [SwaggerOperation(
        Summary = "通过邮箱验证码设置新的登录密码。",
        Description = """
        请求体字段：
        - UserName：RSA 加密后 Base64 编码的邮箱账号。
        - Password：RSA 加密后 Base64 编码的新密码，必须与旧密码不同。
        - VerifyCode：申请验证码时设置 IsResetPassword 标记后收到的验证码。

        注意事项：
        - 若客户端连续三次输入错误验证码，服务器会自动作废验证码，需要重新申请。
        """)]
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

    /// <summary>
    /// 修改绑定邮箱并重新签发访问令牌。
    /// </summary>
    /// <param name="request">
    /// <list type="bullet">
    /// <item>
    /// <description><c>UserName</c> 与 <c>VerifyCode</c>：原邮箱及其验证码。</description>
    /// </item>
    /// <item>
    /// <description><c>NewUserName</c> 与 <c>NewVerifyCode</c>：新邮箱及其验证码，均需使用 RSA 加密后 Base64 编码。</description>
    /// </item>
    /// </list>
    /// </param>
    /// <returns>成功后返回新的访问令牌，失败时返回错误码。</returns>
    /// <remarks>
    /// <para>为避免验证码串用，旧邮箱与新邮箱需分别申请验证码，两组验证码的有效期互相独立。</para>
    /// </remarks>
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

    /// <summary>
    /// 使用邮箱与密码登录并颁发访问令牌与刷新令牌。
    /// </summary>
    /// <param name="request">
    /// <list type="bullet">
    /// <item>
    /// <description><c>UserName</c>：RSA 加密后 Base64 编码的邮箱账号。</description>
    /// </item>
    /// <item>
    /// <description><c>Password</c>：RSA 加密后 Base64 编码的登录密码。</description>
    /// </item>
    /// </list>
    /// </param>
    /// <returns>登录成功时返回 <see cref="TokenResponse"/>，失败时附带错误提示。</returns>
    /// <remarks>
    /// <para>所有登录行为都会记录 <c>DeviceInfo</c>，同一设备重复登录会自动更新刷新令牌。</para>
    /// </remarks>
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

    /// <summary>
    /// 获取当前登录用户的基础资料与资源到期时间。
    /// </summary>
    /// <returns>成功时返回 <see cref="UserInfo"/> 对象，包含账号标识、权限位以及云服务到期时间。</returns>
    /// <remarks>
    /// <para>调用前需携带有效的 Bearer Token，推荐在应用启动时用于刷新本地缓存。</para>
    /// </remarks>
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

    /// <summary>
    /// 查询当前账号所有有效的登录设备。
    /// </summary>
    /// <returns>成功时返回设备列表，包含设备 ID、设备名称、系统类型与登录时间。</returns>
    /// <remarks>
    /// <para>返回数据中的 <c>IsCurrentDevice</c> 字段用于标识调用者自身设备，可据此展示注销按钮。</para>
    /// </remarks>
    [Authorize]
    [HttpGet("LoggedInDevices")]
    public async Task<IActionResult> GetLoggedInDevicesAsync()
    {
        DeviceInfo deviceInfo = this.GetDeviceInfo();
        int userId = this.GetUserId();
        List<LoggedInDeviceInfo> devices = await passportService.GetLoggedInDevicesAsync(userId, deviceInfo).ConfigureAwait(false);
        return Response<List<LoggedInDeviceInfo>>.Success("获取已登录设备成功", devices);
    }

    /// <summary>
    /// 使用刷新令牌换取新的访问令牌。
    /// </summary>
    /// <param name="request">
    /// <list type="bullet">
    /// <item>
    /// <description><c>RefreshToken</c>：Base64 编码的刷新令牌，需要使用 <see cref="PassportService.Decrypt(string)"/> 解密。</description>
    /// </item>
    /// </list>
    /// </param>
    /// <returns>成功时返回新的 <see cref="TokenResponse"/>，失败时返回刷新令牌失效提示。</returns>
    /// <remarks>
    /// <para>刷新接口无需登录态，但应使用 HTTPS 传输，防止令牌被中间人窃取。</para>
    /// </remarks>
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

    /// <summary>
    /// 撤销指定设备的刷新令牌。
    /// </summary>
    /// <param name="request">
    /// <list type="bullet">
    /// <item>
    /// <description><c>DeviceId</c>：RSA 加密后 Base64 编码的设备唯一标识，可从 <see cref="GetLoggedInDevicesAsync"/> 响应中获取。</description>
    /// </item>
    /// </list>
    /// </param>
    /// <returns>撤销结果，成功时返回统一的成功提示消息。</returns>
    /// <remarks>
    /// <para>需要携带当前用户的 Bearer Token，并且只能撤销属于自己的设备。</para>
    /// </remarks>
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

    /// <summary>
    /// 撤销当前账号所有刷新令牌并强制登出所有设备。
    /// </summary>
    /// <returns>统一的成功提示消息。</returns>
    /// <remarks>
    /// <para>该操作可用于账号出现异常登录时的紧急处理。</para>
    /// </remarks>
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
                if (!userExists)
                {
                    passportVerificationService.RemoveVerifyCode(normalizedUserName, PassportVerificationService.CancelRegistration);
                    return Model.Response.Response.Fail(ReturnCode.VerifyCodeNotAllowed, "请求验证码失败", ServerKeys.ServerPassportVerifyRequestUserNotExisted);
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