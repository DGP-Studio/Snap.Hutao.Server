// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class ServiceController
{
    private readonly AppDbContext appDbContext;
    private readonly string key;

    public ServiceController(AppDbContext appDbContext,IConfiguration configuration)
    {
        this.appDbContext = appDbContext;
        key = configuration["Jwt"]!;
    }

    [HttpGet("GachaLog/Compensation")]
    public async Task<IActionResult> GachaLogCompensationAsync([FromQuery] string key, [FromQuery] int days)
    {
        if (this.key != key)
        {
            return Response.Fail(ReturnCode.ServiceKeyInvalid, "密钥不正确");
        }

        long nowTick = DateTimeOffset.Now.ToUnixTimeSeconds();
        await appDbContext.Users
            .Where(user => user.GachaLogExpireAt < nowTick)
            .ExecuteUpdateAsync(user => user.SetProperty(u => u.GachaLogExpireAt, u => nowTick))
            .ConfigureAwait(false);

        int seconds = days * 24 * 60 * 60;

        await appDbContext.Users
            .ExecuteUpdateAsync(user => user.SetProperty(u => u.GachaLogExpireAt, u => nowTick))
            .ConfigureAwait(false);

        nowTick += seconds;
        return Response.Fail(ReturnCode.Success, $"操作成功，增加了 {days} 天时长，到期时间: {DateTimeOffset.FromUnixTimeSeconds(nowTick)}");
    }
}