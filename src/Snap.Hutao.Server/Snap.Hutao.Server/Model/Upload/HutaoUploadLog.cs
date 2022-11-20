// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 胡桃上传的日志
/// </summary>
public class HutaoUploadLog
{
    /// <summary>
    /// 设备Id
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// 崩溃时间
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Info { get; set; } = default!;
}