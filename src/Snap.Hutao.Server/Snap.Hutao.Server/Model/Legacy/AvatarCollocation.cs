// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 角色搭配
/// </summary>
public class AvatarCollocation : AvatarBuild
{
    /// <summary>
    /// 构造一个新的角色搭配
    /// </summary>
    /// <param name="avatarId">角色Id</param>
    public AvatarCollocation(int avatarId)
        : base(avatarId)
    {
    }

    /// <summary>
    /// 其他角色
    /// </summary>
    public List<ItemRate<int, double>> Avatars { get; set; } = default!;

    /// <summary>
    /// 武器
    /// </summary>
    public List<ItemRate<int, double>> Weapons { get; set; } = default!;

    /// <summary>
    /// 圣遗物
    /// </summary>
    public List<ItemRate<string, double>> Reliquaries { get; set; } = default!;
}