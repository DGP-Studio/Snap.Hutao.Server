// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using System.Runtime.InteropServices;

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
    /// 上传者身份
    /// </summary>
    public string Identity { get; set; } = default!;

    /// <summary>
    /// 深境螺旋
    /// </summary>
    public SimpleSpiralAbyss SpiralAbyss { get; set; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    public List<SimpleAvatar> Avatars { get; set; } = default!;

    /// <summary>
    /// 保留属性
    /// 用户名称
    /// </summary>
    public string? ReservedUserName { get; set; }

    /// <summary>
    /// 验证记录有效性
    /// </summary>
    /// <returns>有效性</returns>
    public bool Validate()
    {
        if (Identity == null)
        {
            return false;
        }

        if (Uid == null || Uid.Length != 9)
        {
            return false;
        }

        // 较老的客户端上传的不兼容数据
        if (SpiralAbyss.TotalWinTimes == 0 || SpiralAbyss.TotalBattleTimes == 0)
        {
            return false;
        }

        if (Avatars == null || Avatars.Count <= 8)
        {
            return false;
        }

        // 上下半不完整的楼层
        foreach (ref SimpleFloor floor in CollectionsMarshal.AsSpan(SpiralAbyss.Floors))
        {
            foreach (ref SimpleLevel level in CollectionsMarshal.AsSpan(floor.Levels))
            {
                if (level.Battles.Count != 2)
                {
                    return false;
                }

                HashSet<int> up = level.Battles[0].Avatars.ToHashSet();
                HashSet<int> down = level.Battles[1].Avatars.ToHashSet();

                if (up.Count < level.Battles[0].Avatars.Count || down.Count < level.Battles[1].Avatars.Count)
                {
                    // 上下半中某一半存在重复角色
                    return false;
                }

                up.IntersectWith(down);

                if (up.Count > 0)
                {
                    // 上下半存在重复角色
                    return false;
                }
            }
        }

        return ValidateAvatars();
    }

    private bool ValidateAvatars()
    {
        int traveller = 1;
        int passMainQuest = 3;
        foreach (ref readonly SimpleAvatar avatar in CollectionsMarshal.AsSpan(Avatars))
        {
            if (avatar.WeaponId <= 0 || avatar.WeaponId.StringLength() != 5)
            {
                return false;
            }

            if (avatar.ReliquarySetIds.Any(id => id <= 0 || id.StringLength() != 7))
            {
                return false;
            }

            int avatarId = avatar.AvatarId;

            // 男女主
            // 没有主角的账号100%无效
            if (avatarId is 10000005 or 10000007)
            {
                --traveller;
            }

            // 丽莎/凯亚/安柏
            // 没有御三家的账号可信度不高
            if (avatarId is 10000006 or 10000015 or 10000021)
            {
                --passMainQuest;
            }
        }

        return traveller is 0 && passMainQuest is 0;
    }
}