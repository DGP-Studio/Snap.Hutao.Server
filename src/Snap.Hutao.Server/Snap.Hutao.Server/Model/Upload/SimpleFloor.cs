// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 层信息
/// </summary>
public class SimpleFloor
{
    /// <summary>
    /// 层遍号 1-12|9-12
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 星数
    /// </summary>
    public int Star { get; set; }

    /// <summary>
    /// 间
    /// </summary>
    public List<SimpleLevel> Levels { get; set; } = default!;
}
