// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.GachaLog;

public static class ExpireServiceExtension
{
    public static ValueTask<GachaLogTermExtendResult> ExtendGachaLogTermForAfdianOrderAsync(this ExpireService expireService, AfdianOrderInformation info)
    {
        return expireService.ExtendGachaLogTermForUserNameAsync(info.UserName, 30 * info.OrderCount);
    }
}