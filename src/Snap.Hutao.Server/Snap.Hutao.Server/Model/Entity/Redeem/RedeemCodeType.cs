// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Redeem;

[Flags]
public enum RedeemCodeType : uint
{
    None = 0b0000U,
    TimeLimited = 0b0001U,
    TimesLimited = 0b0010U,
}