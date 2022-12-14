// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 角色相关构筑
/// </summary>
public abstract class AvatarBuild
{
    /// <summary>
    /// 构造一个新的角色构筑信息
    /// </summary>
    [JsonConstructor]
    public AvatarBuild()
    {
    }

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
