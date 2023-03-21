// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Tokens;
using Snap.Hutao.Server.Model.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Snap.Hutao.Server.Service.Authorization;

/// <summary>
/// 令牌服务
/// </summary>
public sealed class JwtTokenService
{
    private readonly SymmetricSecurityKey key;

    /// <summary>
    /// 初始化一个信的Jwt令牌服务
    /// </summary>
    /// <param name="configuration">配置</param>
    public JwtTokenService(IConfiguration configuration)
    {
        key = new(Encoding.UTF8.GetBytes(configuration["Jwt"]!));
    }

    /// <summary>
    /// 创建令牌
    /// </summary>
    /// <param name="user">用户</param>
    /// <returns>令牌</returns>
    public string CreateToken(HutaoUser user)
    {
        JwtSecurityTokenHandler handler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new(new Claim[]
            {
                new Claim(PassportClaimTypes.UserId, user.Id.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new(key, SecurityAlgorithms.HmacSha256Signature),
        };

        SecurityToken token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}