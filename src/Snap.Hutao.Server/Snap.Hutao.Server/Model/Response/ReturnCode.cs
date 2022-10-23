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
}