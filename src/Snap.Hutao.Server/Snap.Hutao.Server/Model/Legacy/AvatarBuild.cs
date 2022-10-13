// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 角色相关解构
/// </summary>
public abstract class AvatarBuild
{
    /// <summary>
    /// 构造一个新的角色构筑信息
    /// </summary>
    /// <param name="avatarId">角色Id</param>
    public AvatarBuild(int avatarId)
    {
        AvatarId = avatarId;
    }

    /// <summary>
    /// 角色Id
    /// </summary>
    public int AvatarId { get; set; }
}
