// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Passport;
using Snap.Hutao.Server.Option;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Service.Authorization;

// Scoped
public sealed class PassportService
{
    private readonly UserManager<HutaoUser> userManager;
    private readonly SymmetricSecurityKey jwtSigningKey;
    private readonly string rsaPrivateKey;

    public PassportService(UserManager<HutaoUser> userManager, AppOptions appOptions)
    {
        this.userManager = userManager;

        jwtSigningKey = appOptions.GetJwtSecurityKey();
        rsaPrivateKey = appOptions.RSAPrivateKey;
    }

    public string Decrypt(string source)
    {
        using (RSA rsa = RSA.Create(2048))
        {
            rsa.ImportFromPem(rsaPrivateKey);
            byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(source), RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    public async Task<PassportResult> RegisterAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            return new(false, "邮箱已被注册", ServerKeys.ServerPassportServiceEmailHasRegistered);
        }

        HutaoUser newUser = new() { UserName = passport.UserName };
        IdentityResult result = await userManager.CreateAsync(newUser, passport.Password).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            StringBuilder messageBuilder = new();

            foreach (IdentityError error in result.Errors)
            {
                messageBuilder.AppendLine($"[{error.Code}]: {error.Description}");
            }

            return new(false, $"注册失败:{messageBuilder}", ServerKeys.ServerPassportServiceInternalException);
        }

        return new(true, "注册成功", ServerKeys.ServerPassportRegisterSucceed, CreateToken(newUser));
    }

    public async Task<PassportResult> ResetPasswordAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            await userManager.RemovePasswordAsync(user).ConfigureAwait(false);
            await userManager.AddPasswordAsync(user, passport.Password).ConfigureAwait(false);

            return new(true, "新密码设置成功", ServerKeys.ServerPassportResetPasswordSucceed, CreateToken(user));
        }
        else
        {
            return new(false, "该邮箱尚未注册", ServerKeys.ServerPassportServiceEmailHasNotRegistered);
        }
    }

    public async Task<PassportResult> LoginAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                return new(true, "登录成功", ServerKeys.ServerPassportLoginSucceed, CreateToken(user));
            }
        }

        return new PassportResult(false, "邮箱或密码不正确", ServerKeys.ServerPassportUserNameOrPasswordIncorrect);
    }

    public async Task<PassportResult> CancelAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                await userManager.DeleteAsync(user).ConfigureAwait(false);
                return new(true, "用户注销成功", ServerKeys.ServerPassportUnregisterSucceed);
            }
        }

        return new PassportResult(false, "用户注销失败", ServerKeys.ServerPassportServiceUnregisterFailed);
    }

    public string CreateTokenByUserId(int userId)
    {
        JwtSecurityTokenHandler handler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new(new Claim[]
            {
                new(PassportClaimTypes.UserId, userId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "homa.snapgenshin.com",
            SigningCredentials = new(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    private string CreateToken(HutaoUser user)
    {
        return CreateTokenByUserId(user.Id);
    }
}