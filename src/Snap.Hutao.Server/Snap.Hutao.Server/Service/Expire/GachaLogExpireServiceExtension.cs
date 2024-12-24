// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.Expire;

public static class GachaLogExpireServiceExtension
{
    public static ValueTask<TermExtendResult> ExtendGachaLogTermForAfdianOrderAsync(this GachaLogExpireService gachaLogExpireService, AfdianOrderInformation info)
    {
        return gachaLogExpireService.ExtendTermForUserNameAsync(info.UserName, 30 * info.OrderCount);
    }
}