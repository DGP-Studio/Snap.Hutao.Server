// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

/// <summary>
/// 角色详情 角色
/// </summary>
public class SimpleAvatar
{
    /// <summary>
    /// 角色 Id
    /// </summary>
    public int AvatarId { get; set; }

    /// <summary>
    /// 武器 Id
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// 圣遗物套装Id
    /// </summary>
    public List<int> ReliquarySetIds { get; set; } = default!;

    /// <summary>
    /// 命座
    /// </summary>
    public int ActivedConstellationNumber { get; set; }
}