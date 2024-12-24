// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Expire;

internal interface IExpireService
{
    ValueTask<TermExtendResult> ExtendTermForUserNameAsync(string userName, int days);

    ValueTask<DateTimeOffset> ExtendTermForAllUsersAsync(int days);
}