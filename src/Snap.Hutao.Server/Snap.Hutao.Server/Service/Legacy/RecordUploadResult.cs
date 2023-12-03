// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy;

public enum RecordUploadResult
{
    OkWithNotFirstAttempt,
    OkWithNotSnapHutaoClient,
    OkWithNoUserNamePresented,
    OkWithGachaLogExtented,
    OkWithGachaLogNoSuchUser,
    None,
    ComputingStatistics,
    UidBanned,
    NotCurrentSchedule,
    InvalidData,
    ConcurrencyNotSupported,
    ConcurrencyStateErrorAdd,
    ConcurrencyStateErrorRemove,
}