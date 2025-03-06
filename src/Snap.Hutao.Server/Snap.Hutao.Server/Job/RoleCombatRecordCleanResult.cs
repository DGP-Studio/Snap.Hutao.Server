// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Job;

public readonly struct RoleCombatRecordCleanResult
{
    public readonly int DeletedNumberOfRecords;

    public RoleCombatRecordCleanResult(int records)
    {
        DeletedNumberOfRecords = records;
    }
}