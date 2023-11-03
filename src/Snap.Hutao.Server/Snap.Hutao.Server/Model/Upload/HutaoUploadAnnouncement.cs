// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity;

namespace Snap.Hutao.Server.Model.Upload;

public sealed class HutaoUploadAnnouncement
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// 严重度
    /// </summary>
    public AnnouncementSeverity Severity { get; set; }

    /// <summary>
    /// 原帖链接
    /// </summary>
    public string Link { get; set; } = default!;

    /// <summary>
    /// 最高的呈现版本
    /// </summary>
    public string? MaxPresentVersion { get; set; }
}