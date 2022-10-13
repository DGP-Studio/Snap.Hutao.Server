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

        if (Avatars == null || Avatars.Count <= 8)
        {
            return false;
        }

        int traveller = 1;
        int passMainQuest = 3;
        foreach (SimpleAvatar a in Avatars)
        {
            int avatarId = a.AvatarId;
            if (avatarId == 10000005 || avatarId == 10000007)
            {
                --traveller;
            }

            // 丽莎/凯亚/安柏
            if (avatarId == 10000006 || avatarId == 10000015 || avatarId == 10000021)
            {
                --passMainQuest;
            }
        }

        return traveller == 0 && passMainQuest == 0;
    }
}
