// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Metadata;

/// <summary>
/// 祈愿卡池配置
/// </summary>
internal sealed class GachaEvent
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset From { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset To { get; set; }

    /// <summary>
    /// 卡池类型
    /// </summary>
    public GachaConfigType Type { get; set; }
}