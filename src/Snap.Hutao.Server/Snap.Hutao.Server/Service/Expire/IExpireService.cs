// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.Expire;

internal interface IExpireService
{
    ValueTask<TermExtendResult> ExtendTermForUserNameAsync(string userName, int days);

    ValueTask<TermExtendResult> ExtendTermForUserAsync(DbSet<HutaoUser> users, HutaoUser user, int days);

    ValueTask<DateTimeOffset> ExtendTermForAllUsersAsync(int days);
}