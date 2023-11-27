// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Licensing;

public enum ApplicationProcessResult
{
    Ok,
    ReCaptchaVerificationFailed,
    UsetNotExists,
    AlreadyApplied,
}