// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 记录
/// </summary>
public class SimpleRecord
{
    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// 深境螺旋
    /// </summary>
    public SimpleSpiralAbyss SpiralAbyss { get; set; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    public List<SimpleAvatar> Avatars { get; set; } = default!;
}
