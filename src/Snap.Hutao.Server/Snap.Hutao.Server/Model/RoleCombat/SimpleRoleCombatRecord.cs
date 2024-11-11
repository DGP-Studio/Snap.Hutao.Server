// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.RoleCombat;

public sealed class SimpleRoleCombatRecord
{
    public uint Version { get; set; }

    #region V1
    public string Uid { get; set; } = default!;

    public string Identity { get; set; } = default!;

    public List<uint> BackupAvatars { get; set; } = default!;

    public int ScheduleId { get; set; }
    #endregion

    public bool Validate()
    {
        if (Version != 1)
        {
            return false;
        }

        if (Uid is null || Uid.Length < 9)
        {
            return false;
        }

        if (BackupAvatars is not { Count: >= 8 })
        {
            return false;
        }

        return true;
    }
}