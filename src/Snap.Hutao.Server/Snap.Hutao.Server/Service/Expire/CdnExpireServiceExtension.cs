// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.Expire;

public static class CdnExpireServiceExtension
{
    public static ValueTask<TermExtendResult> ExtendCdnTermForAfdianOrderAsync(this CdnExpireService cdnExpireService, AfdianOrderInformation info)
    {
        return cdnExpireService.ExtendTermForUserNameAsync(info.UserName, 30 * info.OrderCount);
    }
}