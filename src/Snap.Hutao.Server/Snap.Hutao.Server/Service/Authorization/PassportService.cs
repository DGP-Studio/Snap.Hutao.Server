// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Passport;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 通行证服务
/// </summary>
public sealed class PassportService
{
    private readonly UserManager<HutaoUser> userManager;
    private readonly string rsaPrivateKey;
    private readonly SymmetricSecurityKey jwtSigningKey;

    public PassportService(UserManager<HutaoUser> userManager, AppOptions appOptions)
    {
        this.userManager = userManager;

        rsaPrivateKey = appOptions.RSAPrivateKey;
        jwtSigningKey = appOptions.JwtSecurityKey;
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="source">密文</param>
    /// <returns>解密的文本</returns>
    public string Decrypt(string source)
    {
        byte[] encryptedBytes = Convert.FromBase64String(source);
        using (RSACryptoServiceProvider rsa = new(2048))
        {
            rsa.ImportFromPem(rsaPrivateKey);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, true);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    /// <summary>
    /// 异步注册
    /// </summary>
    /// <param name="passport">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> RegisterAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            return new(false, "邮箱已被注册", "ServerPassportServiceEmailHasRegistered");
        }
        else
        {
            HutaoUser newUser = new() { UserName = passport.UserName };
            IdentityResult result = await userManager.CreateAsync(newUser, passport.Password).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return new(true, "注册成功", null, CreateToken(newUser));
            }
            else
            {
                StringBuilder messageBuilder = new();

                foreach (IdentityError error in result.Errors)
                {
                    messageBuilder.AppendLine($"[{error.Code}]: {error.Description}");
                }

                return new(false, $"注册失败:{messageBuilder}", "ServerPassportServiceInternalException");
            }
        }
    }

    /// <summary>
    /// 异步修改密码
    /// </summary>
    /// <param name="passport">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> ResetPasswordAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            await userManager.RemovePasswordAsync(user).ConfigureAwait(false);
            await userManager.AddPasswordAsync(user, passport.Password).ConfigureAwait(false);

            return new(true, "新密码设置成功", null, CreateToken(user));
        }
        else
        {
            return new(false, "该邮箱尚未注册", "ServerPassportServiceEmailHasNotRegistered");
        }
    }

    /// <summary>
    /// 异步登录
    /// </summary>
    /// <param name="passport">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> LoginAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                return new(true, "登录成功", null, CreateToken(user));
            }
        }

        return new PassportResult(false, "邮箱或密码不正确", "ServerPassportUsernameOrPassportIncorrect");
    }

    /// <summary>
    /// 异步注销
    /// </summary>
    /// <param name="passport">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> CancelAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                await userManager.DeleteAsync(user).ConfigureAwait(false);
                return new(true, "用户注销成功", null);
            }
        }

        return new PassportResult(false, "用户注销失败", "ServerPassportServiceUnregisterFailed");
    }

    private string CreateToken(HutaoUser user)
    {
        JwtSecurityTokenHandler handler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new(new Claim[]
            {
                new Claim(PassportClaimTypes.UserId, user.Id.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "homa.snapgenshin.com",
            SigningCredentials = new(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}