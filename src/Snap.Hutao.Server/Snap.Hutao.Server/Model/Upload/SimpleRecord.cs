// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Model.Upload;

public sealed class SimpleRecord
{
    public string Uid { get; set; } = default!;

    public string Identity { get; set; } = default!;

    public SimpleSpiralAbyss SpiralAbyss { get; set; } = default!;

    public List<SimpleAvatar> Avatars { get; set; } = default!;

    public string? ReservedUserName { get; set; }

    public bool Validate()
    {
        if (Identity == null)
        {
            return false;
        }

        if (Uid == null || Uid.Length < 9)
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

        HashSet<int> spiralAbyssPresented = [];

        // 上下半不完整的楼层
        foreach (ref SimpleFloor floor in CollectionsMarshal.AsSpan(SpiralAbyss.Floors))
        {
            if (floor.Index < 9)
            {
                continue;
            }

            foreach (ref SimpleLevel level in CollectionsMarshal.AsSpan(floor.Levels))
            {
                if (level.Battles.Count != 2)
                {
                    return false;
                }

                HashSet<int> up = [.. level.Battles[0].Avatars];
                HashSet<int> down = [.. level.Battles[1].Avatars];
                spiralAbyssPresented.UnionWith(up);
                spiralAbyssPresented.UnionWith(down);
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

        return ValidateAvatars(spiralAbyssPresented);
    }

    private bool ValidateAvatars(HashSet<int> spiralAbyssPresented)
    {
        HashSet<int> uidOwns = [];
        int traveller = 1;
        int passMainQuest = 3;
        foreach (ref readonly SimpleAvatar avatar in CollectionsMarshal.AsSpan(Avatars))
        {
            uidOwns.Add(avatar.AvatarId);

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
            // 没有御三家的账号直接拒绝
            if (avatarId is 10000006 or 10000015 or 10000021)
            {
                --passMainQuest;
            }
        }

        // 深渊记录中存在角色列表中不存在的角色
        spiralAbyssPresented.ExceptWith(uidOwns);
        if (spiralAbyssPresented.Count > 0)
        {
            return false;
        }

        return traveller is 0 && passMainQuest is 0;
    }
}