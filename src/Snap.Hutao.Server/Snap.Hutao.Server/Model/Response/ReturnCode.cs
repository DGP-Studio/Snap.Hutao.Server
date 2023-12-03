// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Response;

// |5| X | Y |  ZZZ  |
// |5|1-5|1-6|000-999|
// -----------------------------
// X: 1 - Invalid Request
//    2 - Request Frequency
//    3 - Limitation for User
//    4 - Internal Waiting
//    5 - Internal Exception
// Y: 1 - General
//    2 - SpiralAbyss Statistics
//    3 - GachaLog Service
//    4 - Passport
//    5 - Licensing
//    6 - Hosting
[SuppressMessage("", "SA1025")]
public enum ReturnCode
{
    Success = 0,

    InvalidUploadData           = 511001,
    RequestTooFrequently        = 521001,
    PreviousRequestNotCompleted = 541001,
    InternalStateException      = 551001,
    InvalidQueryString          = 511002,

    BannedUid                   = 512001,
    ComputingStatistics         = 542001,
    NotCurrentSchedule          = 512002,

    GachaLogServiceNotAllowed   = 513001,
    TooManyGachaLogUids         = 533001,
    GachaLogDbException         = 553001,
    InvalidGachaLogItems        = 513002,
    GachaLogExtendDbException   = 553002,

    RegisterFail                = 514001,
    VerifyCodeTooFrequently     = 524001,
    VerifyCodeNotAllowed        = 544001,
    LoginFail                   = 514002,
    CancelFail                  = 514003,

    ReCaptchaVerifyFailed       = 515001,
    AlreadyAppliedForLicense    = 545001,
    LicenseApprovalFailed       = 555001,

    ServiceKeyInvalid           = 516001,
    UserNameNotExists           = 516002,
}