// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.StaticResource;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "StaticResource")]
public class StaticResourceController : ControllerBase
{
    private readonly StaticResourceService staticResourceService;
    private readonly AppDbContext appDbContext;

    public StaticResourceController(IServiceProvider serviceProvider)
    {
        staticResourceService = serviceProvider.GetRequiredService<StaticResourceService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    [Authorize]
    [ServiceFilter(typeof(ValidateCdnPermission))]
    [HttpGet("GetAcceleratedImageToken")]
    public async Task<IActionResult> GetAcceleratedImageToken()
    {
        if (await this.GetUserAsync(appDbContext.Users).ConfigureAwait(false) is not { } user)
        {
            return Model.Response.Response.Fail(ReturnCode.NoUserIdentity, "请先登录或注册胡桃账号");
        }

        ArgumentNullException.ThrowIfNull(user.UserName);

        ImageToken? token = await staticResourceService.GetAcceleratedImageTokenAsync(user.UserName);
        return Response<ImageToken?>.Success("获取图片加速镜像Token成功", token);
    }
}