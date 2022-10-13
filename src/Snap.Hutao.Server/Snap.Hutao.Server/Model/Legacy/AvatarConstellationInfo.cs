// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 角色命座信息
/// </summary>
public class AvatarConstellationInfo : AvatarBuild
{
    /// <summary>
    /// 构造一个新的角色命座信息
    /// </summary>
    /// <param name="avatarId">角色Id</param>
    public AvatarConstellationInfo(int avatarId)
        : base(avatarId)
    {
    }

    /// <summary>
    /// 持有率
    /// </summary>
    public double HoldingRate { get; set; }

    /// <summary>
    /// 命之座
    /// </summary>
    public List<ItemRate<int, double>> Constellations { get; set; } = default!;
}