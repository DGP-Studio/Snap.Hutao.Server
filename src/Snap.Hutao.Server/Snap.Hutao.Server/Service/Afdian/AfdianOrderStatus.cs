// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Afdian;

public enum AfdianOrderStatus
{
    Ok,
    NoSkuDetails,
    SkuIdNotSupported,
    ValidationRequestFailed,
    ValidationResponseInvalid,
    ValidationResponseNoOrder,
    ValidationResponseNoSkuDetail,
    ValidationResponseSkuDetailNotMatch,
    GachaLogTermExtendNoSuchUser,
    GachaLogTermExtendDbError,
    CdnTermExtendNoSuchUser,
    CdnTermExtendDbError,
    InvalidUserName,
}