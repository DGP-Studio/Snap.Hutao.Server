// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.GachaLog;

// Singleton
public sealed class ExpireService
{
    private readonly IServiceProvider serviceProvider;

    public ExpireService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async ValueTask<GachaLogTermExtendResult> ExtendGachaLogTermForUserNameAsync(string userName, int days)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            UserManager<HutaoUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<HutaoUser>>();

            HutaoUser? user = await userManager.FindByNameAsync(userName).ConfigureAwait(false);

            if (user == null)
            {
                return new(GachaLogTermExtendResultKind.NoSuchUser);
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (user.GachaLogExpireAt < now)
            {
                user.GachaLogExpireAt = now;
            }

            user.GachaLogExpireAt += (long)TimeSpan.FromDays(days).TotalSeconds;

            IdentityResult result = await userManager.UpdateAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return new(GachaLogTermExtendResultKind.DbError);
            }

            return new(GachaLogTermExtendResultKind.Ok, DateTimeOffset.FromUnixTimeSeconds(user.GachaLogExpireAt));
        }
    }

    public async ValueTask<DateTimeOffset> ExtendGachaLogTermForAllUsersAsync(int days)
    {
        long nowTick = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int seconds = days * 24 * 60 * 60;

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await appDbContext.Users
                .Where(user => user.GachaLogExpireAt < nowTick)
                .ExecuteUpdateAsync(user => user.SetProperty(u => u.GachaLogExpireAt, u => nowTick))
                .ConfigureAwait(false);

            await appDbContext.Users
                .ExecuteUpdateAsync(user => user.SetProperty(u => u.GachaLogExpireAt, u => u.GachaLogExpireAt + seconds))
                .ConfigureAwait(false);
        }

        nowTick += seconds;
        return DateTimeOffset.FromUnixTimeSeconds(nowTick);
    }
}