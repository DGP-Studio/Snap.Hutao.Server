// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Controller.Helper;

/// <summary>
/// 记录上传结果
/// </summary>
public enum RecordUploadResult
{
    /// <summary>
    /// 无状态
    /// </summary>
    None,

    /// <summary>
    /// 未提供用户名
    /// </summary>
    NoUserNamePresented,

    /// <summary>
    /// 非本轮首次上传
    /// </summary>
    NotFirstAttempt,

    /// <summary>
    /// 非胡桃客户端上传
    /// </summary>
    NotSnapHutao,

    /// <summary>
    /// 成功
    /// </summary>
    GachaLogExtented,
}