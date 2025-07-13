// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
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
    private const int AccessTokenExpirationMinutes = /* 15 */ 3 * 60;
    private const int RefreshTokenExpirationDays = 30;

    private readonly UserManager<HutaoUser> userManager;
    private readonly AppDbContext appDbContext;
    private readonly SymmetricSecurityKey jwtSigningKey;
    private readonly string rsaPrivateKey;

    public PassportService(UserManager<HutaoUser> userManager, AppDbContext appDbContext, AppOptions appOptions)
    {
        this.userManager = userManager;
        this.appDbContext = appDbContext;

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

    public async Task<PassportResult> RegisterAsync(Passport passport, DeviceInfo? deviceInfo)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is not null)
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

        if (!await appDbContext.RegistrationRecords.AnyAsync(r => r.UserName == passport.UserName).ConfigureAwait(false))
        {
            await appDbContext.RegistrationRecords.AddAndSaveAsync(new() { UserName = passport.UserName }).ConfigureAwait(false);

            newUser.CdnExpireAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)TimeSpan.FromDays(3).TotalSeconds;
            newUser.GachaLogExpireAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)TimeSpan.FromDays(3).TotalSeconds;
            await userManager.UpdateAsync(newUser).ConfigureAwait(false);
        }

        TokenResponse tokenResponse = await CreateTokenResponseAsync(newUser, deviceInfo).ConfigureAwait(false);
        return new(true, "注册成功", ServerKeys.ServerPassportRegisterSucceed, tokenResponse);
    }

    public async Task<PassportResult> ResetPasswordAsync(Passport passport, DeviceInfo? deviceInfo)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is { } user)
        {
            await userManager.RemovePasswordAsync(user).ConfigureAwait(false);
            await userManager.AddPasswordAsync(user, passport.Password).ConfigureAwait(false);

            TokenResponse tokenResponse = await CreateTokenResponseAsync(user, deviceInfo).ConfigureAwait(false);
            return new(true, "新密码设置成功", ServerKeys.ServerPassportResetPasswordSucceed, tokenResponse);
        }

        return new(false, "该邮箱尚未注册", ServerKeys.ServerPassportServiceEmailHasNotRegistered);
    }

    public async Task<PassportResult> ResetUsernameAsync(Passport passport, DeviceInfo? deviceInfo)
    {
        if (await userManager.FindByNameAsync(passport.NewUserName).ConfigureAwait(false) is not null)
        {
            return new(false, "邮箱已被注册", ServerKeys.ServerPassportServiceEmailHasRegistered);
        }

        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is { } user)
        {
            await userManager.SetUserNameAsync(user, passport.NewUserName).ConfigureAwait(false);

            if (!await appDbContext.RegistrationRecords.AnyAsync(r => r.UserName == passport.NewUserName).ConfigureAwait(false))
            {
                await appDbContext.RegistrationRecords.AddAndSaveAsync(new() { UserName = passport.NewUserName }).ConfigureAwait(false);
            }

            TokenResponse tokenResponse = await CreateTokenResponseAsync(user, deviceInfo).ConfigureAwait(false);
            return new(true, "邮箱修改成功", ServerKeys.ServerPassportResetUserNameSucceed, tokenResponse);
        }

        return new(false, "该邮箱尚未注册", ServerKeys.ServerPassportServiceEmailHasNotRegistered);
    }

    public async Task<PassportResult> LoginAsync(Passport passport, DeviceInfo? deviceInfo)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is { } user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                TokenResponse tokenResponse = await CreateTokenResponseAsync(user, deviceInfo).ConfigureAwait(false);
                return new(true, "登录成功", ServerKeys.ServerPassportLoginSucceed, tokenResponse);
            }
        }

        return new(false, "邮箱或密码不正确", ServerKeys.ServerPassportUserNameOrPasswordIncorrect);
    }

    public async Task<PassportResult> CancelAsync(Passport passport)
    {
        if (await userManager.FindByNameAsync(passport.UserName).ConfigureAwait(false) is { } user)
        {
            if (await userManager.CheckPasswordAsync(user, passport.Password).ConfigureAwait(false))
            {
                await userManager.DeleteAsync(user).ConfigureAwait(false);
                return new(true, "用户注销成功", ServerKeys.ServerPassportUnregisterSucceed);
            }

            return new(false, "用户注销失败", ServerKeys.ServerPassportServiceUnregisterPasswordIncorrect);
        }

        return new PassportResult(false, "用户注销失败", ServerKeys.ServerPassportServiceUnregisterUserNotFound);
    }

    public string CreateTokenByUserId(int userId)
    {
        JwtSecurityTokenHandler handler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new([new(PassportClaimTypes.UserId, userId.ToString())]),
            Expires = DateTime.UtcNow.AddHours(3),
            Issuer = "homa.snapgenshin.com",
            SigningCredentials = new(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    public async Task<List<LoggedInDeviceInfo>> GetLoggedInDevicesAsync(int userId, DeviceInfo deviceInfo)
    {
        return await appDbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .Select(rt => new LoggedInDeviceInfo
            {
                DeviceId = rt.DeviceInfo.DeviceId,
                DeviceName = rt.DeviceInfo.DeviceName,
                LastLoginAt = rt.CreatedAt,
                ExpiresAt = rt.ExpiresAt,
                IsCurrentDevice = rt.DeviceInfo.DeviceId == deviceInfo.DeviceId,
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<TokenResponse?> RefreshTokenAsync(string refreshTokenValue)
    {
        RefreshToken? refreshToken = await appDbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

        if (refreshToken is null || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return default;
        }

        await appDbContext.RefreshTokens.RemoveAndSaveAsync(refreshToken).ConfigureAwait(false);
        return await CreateTokenResponseAsync(refreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string deviceId)
    {
        RefreshToken? refreshToken = await appDbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.DeviceInfo.DeviceId == deviceId);

        if (refreshToken == null)
        {
            return false;
        }

        await appDbContext.RefreshTokens.RemoveAndSaveAsync(refreshToken).ConfigureAwait(false);

        return true;
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        await appDbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public Task<TokenResponse> CreateTokenResponseAsync(RefreshToken refreshToken)
    {
        return CreateTokenResponseAsync(refreshToken.UserId, refreshToken.DeviceInfo);
    }

    public Task<TokenResponse> CreateTokenResponseAsync(HutaoUser user, DeviceInfo? deviceInfo)
    {
        return CreateTokenResponseAsync(user.Id, deviceInfo);
    }

    public async Task<TokenResponse> CreateTokenResponseAsync(int userId, DeviceInfo? deviceInfo)
    {
        string accessToken = CreateAccessToken(userId, deviceInfo);
        if (deviceInfo is null)
        {
            return new()
            {
                AccessToken = accessToken,
                RefreshToken = default!,
                ExpiresIn = AccessTokenExpirationMinutes * 60,
            };
        }

        RefreshToken refreshToken = await CreateRefreshTokenAsync(userId, deviceInfo);

        return new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = AccessTokenExpirationMinutes * 60,
        };
    }

    private static unsafe string GenerateRefreshToken()
    {
        Span<byte> randomBytes = stackalloc byte[64];
        RandomNumberGenerator.Create().GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string CreateAccessToken(int userId, DeviceInfo? deviceInfo)
    {
        JwtSecurityTokenHandler handler = new();
        ClaimsIdentity claimsIdentity = new ClaimsIdentity([
            new(PassportClaimTypes.UserId, userId.ToString()),
        ]);

        if (deviceInfo is not null)
        {
            claimsIdentity.AddClaim(new(JwtRegisteredClaimNames.Jti, deviceInfo.DeviceId));
        }

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            Issuer = "homa.snapgenshin.com",
            SigningCredentials = new(jwtSigningKey, SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(int userId, DeviceInfo deviceInfo)
    {
        RefreshToken refreshToken = new()
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo,
        };

        await appDbContext.RefreshTokens.AddAndSaveAsync(refreshToken);
        return refreshToken;
    }
}