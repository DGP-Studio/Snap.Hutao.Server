// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Redeem;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(CountRequests))]
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
            HutaoUser? user = await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false);
            if (user is null)
            {
                return Model.Response.Response.Fail(ReturnCode.UserNotLogin, "未登录");
            }

            req.Username = user.UserName;
        }

        RedeemUseResponse response = await this.redeemService.UseRedeemCodeAsync(req).ConfigureAwait(false);
        return Response<RedeemUseResponse>.Success("兑换成功", response);
    }
}