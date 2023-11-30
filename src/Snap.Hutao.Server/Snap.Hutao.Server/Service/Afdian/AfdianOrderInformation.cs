// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Afdian;

public class AfdianOrderInformation
{
    public AfdianOrderStatus Status { get; set; }

    public string SkuId { get; set; } = default!;

    public string OrderNumber { get; set; } = default!;

    public int OrderCount { get; set; }

    public string UserName { get; set; } = default!;
}