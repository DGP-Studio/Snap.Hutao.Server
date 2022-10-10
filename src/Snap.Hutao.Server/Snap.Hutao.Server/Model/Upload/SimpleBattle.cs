// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 上下半信息
/// </summary>
public class SimpleBattle
{
    /// <summary>
    /// 上下半遍号 1-2
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<int> Avatars { get; set; } = default!;
}