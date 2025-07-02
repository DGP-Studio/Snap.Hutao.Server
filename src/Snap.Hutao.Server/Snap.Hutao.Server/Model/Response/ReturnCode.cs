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
//    7 - CDN
//    8 - Redeem
[SuppressMessage("", "SA1025")]
public enum ReturnCode
{
    Success = 0,
    InvalidUploadData = 511001,
    InvalidQueryString = 511002,
    BannedUid = 512001,
    NotCurrentSchedule = 512002,
    GachaLogServiceNotAllowed = 513001,
    InvalidGachaLogItems = 513002,
    RegisterFail = 514001,
    LoginFail = 514002,
    CancelFail = 514003,
    InternalGithubAuthException = 514004,
    InvalidGithubAuthState = 514005,
    GithubAlreadyAuthorized = 514006,
    GithubAuthorizationCanceled = 514007,
    NoUserIdentity = 514008,
    InvalidUserName = 514009,
    TooShortPassword = 514010,
    ReCaptchaVerifyFailed = 515001,
    ServiceKeyInvalid = 516001,
    UserNameNotExists = 516002,
    RedeemCodeInvalid = 518001,
    RedeemCodeNotExists = 518002,
    RedeemCodeAlreadyUsed = 518003,
    RedeemCodeExpired = 518004,
    RedeemCodeNotEnough = 518005,
    RedeemCodeNoSuchUser = 518006,

    RequestTooFrequently = 521001,
    VerifyCodeTooFrequently = 524001,

    TooManyGachaLogUids = 533001,

    PreviousRequestNotCompleted = 541001,
    ComputingStatistics = 542001,
    VerifyCodeNotAllowed = 544001,
    AlreadyAppliedForLicense = 545001,

    InternalStateException = 551001,
    GachaLogDbException = 553001,
    GachaLogExtendDbException = 553002,
    LicenseApprovalFailed = 555001,
    CdnExtendDbException = 557001,
    CdnDispatcherException = 557002,
    RedeemCodeDbException = 558001,
}