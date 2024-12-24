// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.Distribution;

// Singleton
public sealed class CdnExpireService
{
    private readonly IServiceProvider serviceProvider;

    public CdnExpireService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async ValueTask<CdnTermExtendResult> ExtendCdnTermForUserNameAsync(string userName, int days)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            UserManager<HutaoUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<HutaoUser>>();

            HutaoUser? user = await userManager.FindByNameAsync(userName).ConfigureAwait(false);

            if (user == null)
            {
                return new(CdnTermExtendResultKind.NoSuchUser);
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (user.CdnExpireAt < now)
            {
                user.CdnExpireAt = now;
            }

            user.CdnExpireAt += (long)TimeSpan.FromDays(days).TotalSeconds;

            IdentityResult result = await userManager.UpdateAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return new(CdnTermExtendResultKind.DbError);
            }

            return new(CdnTermExtendResultKind.Ok, DateTimeOffset.FromUnixTimeSeconds(user.CdnExpireAt));
        }
    }

    public async ValueTask<DateTimeOffset> ExtendCdnTermForAllUsersAsync(int days)
    {
        long nowTick = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int seconds = days * 24 * 60 * 60;

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await appDbContext.Users
                .Where(user => user.CdnExpireAt < nowTick)
                .ExecuteUpdateAsync(user => user.SetProperty(u => u.CdnExpireAt, u => nowTick))
                .ConfigureAwait(false);

            await appDbContext.Users
                .ExecuteUpdateAsync(user => user.SetProperty(u => u.CdnExpireAt, u => u.CdnExpireAt + seconds))
                .ConfigureAwait(false);
        }

        nowTick += seconds;
        return DateTimeOffset.FromUnixTimeSeconds(nowTick);
    }
}