// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Identity;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Passport;
using System.Security.Cryptography;
using System.Text;

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 通行证服务
/// </summary>
public class PassportService
{
    private readonly JwtTokenService jwtTokenService;
    private readonly UserManager<HutaoUser> userManager;
    private readonly string rsaPrivateKey;

    /// <summary>
    /// 构造一个新的通行证服务
    /// </summary>
    /// <param name="jwtTokenService">jwt令牌服务</param>
    /// <param name="userManager">用户管理器</param>
    /// <param name="configuration">配置</param>
    public PassportService(JwtTokenService jwtTokenService, UserManager<HutaoUser> userManager, IConfiguration configuration)
    {
        this.jwtTokenService = jwtTokenService;
        this.userManager = userManager;

        rsaPrivateKey = configuration["RSAPrivateKey"]!;
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
    /// <param name="ingestion">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> RegisterAsync(Passport ingestion)
    {
        if (await userManager.FindByNameAsync(ingestion.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            return new(false, "邮箱已被注册");
        }
        else
        {
            HutaoUser newUser = new() { UserName = ingestion.UserName };
            IdentityResult result = await userManager.CreateAsync(newUser, ingestion.Password).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return new(true, "注册成功", jwtTokenService.CreateToken(newUser));
            }
            else
            {
                return new(false, "数据库错误");
            }
        }
    }

    /// <summary>
    /// 异步登录
    /// </summary>
    /// <param name="ingestion">账密</param>
    /// <returns>结果</returns>
    public async Task<PassportResult> LoginAsync(Passport ingestion)
    {
        if (await userManager.FindByNameAsync(ingestion.UserName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await userManager.CheckPasswordAsync(user, ingestion.Password).ConfigureAwait(false))
            {
                return new(true, "登录成功", jwtTokenService.CreateToken(user));
            }
        }

        return new PassportResult(false, "邮箱或密码不正确");
    }
}