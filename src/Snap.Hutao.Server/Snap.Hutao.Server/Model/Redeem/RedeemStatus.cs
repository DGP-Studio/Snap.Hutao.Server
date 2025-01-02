// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Redeem;

public enum RedeemStatus
{
    Ok,
    Invalid,
    NotExists,
    AlreadyUsed,
    Expired,
    NotEnough,
    NoSuchUser,
    DbError,
}