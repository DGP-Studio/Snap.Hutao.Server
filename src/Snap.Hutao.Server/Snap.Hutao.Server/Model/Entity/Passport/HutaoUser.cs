// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Redeem;

namespace Snap.Hutao.Server.Model.Entity.Passport;

public sealed class HutaoUser : IdentityUser<int>
{
    public long GachaLogExpireAt { get; set; }

    public long CdnExpireAt { get; set; }

    public bool IsLicensedDeveloper { get; set; }

    public bool IsMaintainer { get; set; }

    public List<RedeemCodeUseItem> RedeemCodeUseItems { get; set; } = default!;
}