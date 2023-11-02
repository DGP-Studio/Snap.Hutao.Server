// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Response;

/// <summary>
/// ApiCode
/// </summary>
public enum ReturnCode
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 计算统计数据中
    /// </summary>
    ComputingStatistics = 500000,

    /// <summary>
    /// 内部状态异常
    /// </summary>
    InternalStateException = 500001,

    /// <summary>
    /// 无对应的日志Id
    /// </summary>
    NoMatchedLogId = 500002,

    /// <summary>
    /// 上个请求尚未结束
    /// </summary>
    PreviousRequestNotCompleted = 500010,

    /// <summary>
    /// 请求过于频繁
    /// </summary>
    RequestTooFrequent = 500011,

    /// <summary>
    /// 无效的数据
    /// </summary>
    InvalidUploadData = 500020,

    /// <summary>
    /// 无效的查询
    /// </summary>
    InvalidQueryString = 500021,

    /// <summary>
    /// Uid 由于某些原因被我们封禁
    /// </summary>
    BannedUid = 500022,

    /// <summary>
    /// 非当期深渊数据
    /// </summary>
    NotCurrentSchedule = 500023,

    /// <summary>
    /// 注册失败
    /// </summary>
    RegisterFail = 500030,

    /// <summary>
    /// 登录失败
    /// </summary>
    LoginFail = 500031,

    /// <summary>
    /// 请求验证码过于频繁
    /// </summary>
    VerifyCodeTooFrequently = 500032,

    /// <summary>
    /// reCAPTCHA 验证失败
    /// </summary>
    ReCaptchaVerificationFailed = 500033,

    /// <summary>
    /// 已经申请了许可
    /// </summary>
    AlreadyAppliedForLicense = 500034,

    /// <summary>
    /// 许可批准失败
    /// </summary>
    LicenseApprovalFailed = 500035,

    /// <summary>
    /// 注册失败
    /// </summary>
    CancelFail = 500036,

    /// <summary>
    /// 无法请求验证码
    /// </summary>
    VerifyCodeNotAllowed = 500037,

    /// <summary>
    /// 祈愿记录数据库操作失败
    /// </summary>
    GachaLogDatabaseOperationFailed = 500040,

    /// <summary>
    /// 祈愿记录服务无法使用
    /// </summary>
    GachaLogServiceNotAllowed = 500041,

    /// <summary>
    /// 祈愿记录服务无法使用
    /// </summary>
    InvalidGachaLogItems = 500042,

    /// <summary>
    /// Uid 超出个数限制
    /// </summary>
    TooManyGachaLogUids = 500043,

    /// <summary>
    /// Key 不正确
    /// </summary>
    ServiceKeyInvalid = 500050,

    /// <summary>
    /// 用户名不存在
    /// </summary>
    UserNameNotExists = 500051,
}