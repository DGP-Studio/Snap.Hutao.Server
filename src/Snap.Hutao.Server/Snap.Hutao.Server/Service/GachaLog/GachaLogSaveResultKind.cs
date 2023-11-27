// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.GachaLog;

public enum GachaLogSaveResultKind
{
    Ok,
    UidPerUserLimitExceeded,
    InvalidGachaItemDetected,
    DatebaseOperationFailed,
}