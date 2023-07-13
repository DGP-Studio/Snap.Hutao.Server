// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Service;

/// <summary>
/// 续期服务
/// </summary>
public sealed class ExpireService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ExpireService> logger;

    /// <summary>
    /// 构造一个新的续期服务
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public ExpireService(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetRequiredService<ILogger<ExpireService>>();
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 延长祈愿记录上传服务时间
    /// </summary>
    /// <param name="userName">用户名称</param>
    /// <param name="days">天数</param>
    /// <param name="onSucceed">成功后执行</param>
    /// <returns>任务</returns>
    public async Task<bool> ExtendGachaLogTermAsync(string userName, int days, Func<HutaoUser, Task>? onSucceed = null)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            UserManager<HutaoUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<HutaoUser>>();

            HutaoUser? user = await userManager.FindByNameAsync(userName).ConfigureAwait(false);

            if (user != null)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (user.GachaLogExpireAt < now)
                {
                    user.GachaLogExpireAt = now;
                }

                user.GachaLogExpireAt += (long)TimeSpan.FromDays(days).TotalSeconds;

                IdentityResult result = await userManager.UpdateAsync(user).ConfigureAwait(false);

                if (result.Succeeded)
                {
                    if (onSucceed != null)
                    {
                        await onSucceed(user).ConfigureAwait(false);
                    }

                    return true;
                }
                else
                {
                    logger.LogInformation("Update db failed");
                }
            }
            else
            {
                logger.LogInformation("No such user: {user}", userName);
            }

            return false;
        }
    }
}