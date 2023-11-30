// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Option;

public sealed class GenshinPizzaHelperOptions
{
    public string UidSalt { get; set; } = default!;

    public string ApiSalt { get; set; } = default!;

    public GenshinPizzaHelperEndPointsOptions EndPoints { get; set; } = default!;
}