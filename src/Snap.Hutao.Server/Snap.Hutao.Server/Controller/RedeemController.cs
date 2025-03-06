// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Redeem;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
public class RedeemController : ControllerBase
{
    private readonly RedeemService redeemService;
    private readonly AppDbContext appDbContext;

    public RedeemController(IServiceProvider serviceProvider)
    {
        redeemService = serviceProvider.GetRequiredService<RedeemService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    [HttpPost("Use")]
    public async Task<IActionResult> UseRedeemCodeAsync([FromBody] RedeemUseRequest req)
    {
        if (req.Username is null)
        {
            if (await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false) is not { } user)
            {
                return Model.Response.Response.Fail(ReturnCode.NoUserIdentity, "请先登录或注册胡桃账号");
            }

            req.Username = user.UserName;
        }

        RedeemUseResponse response = await redeemService.UseRedeemCodeAsync(req).ConfigureAwait(false);

        return response.Status switch
        {
            RedeemStatus.Ok => Response<RedeemUseResponse>.Success($"兑换成功，获得 {response.Value} 天 {response.Type.ToServiceName()} 服务权限，到期时间: {response.ExpireTime:yyyy/M/d hh:mm:ss}", response),
            RedeemStatus.Invalid => Model.Response.Response.Fail(ReturnCode.RedeemCodeInvalid, "兑换码无效"),
            RedeemStatus.NotExists => Model.Response.Response.Fail(ReturnCode.RedeemCodeNotExists, "兑换码不存在"),
            RedeemStatus.AlreadyUsed => Model.Response.Response.Fail(ReturnCode.RedeemCodeAlreadyUsed, "兑换码已被使用"),
            RedeemStatus.Expired => Model.Response.Response.Fail(ReturnCode.RedeemCodeExpired, "兑换码已过期"),
            RedeemStatus.NotEnough => Model.Response.Response.Fail(ReturnCode.RedeemCodeNotEnough, "兑换码剩余次数不足"),
            RedeemStatus.NoSuchUser => Model.Response.Response.Fail(ReturnCode.RedeemCodeNoSuchUser, "用户不存在"),
            RedeemStatus.DbError => Model.Response.Response.Fail(ReturnCode.RedeemCodeDbException, "数据库错误"),
            _ => throw new NotSupportedException(),
        };
    }
}