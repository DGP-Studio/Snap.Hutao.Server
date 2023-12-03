// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Job;

public readonly struct SpiralAbyssRecordCleanResult
{
    public readonly int DeletedNumberOfRecords;
    public readonly int DeletedNumberOfSpiralAbysses;
    public readonly long RemovedNumberOfRedisKeys;

    public SpiralAbyssRecordCleanResult(int records, int spiralAbyss, long redis)
    {
        DeletedNumberOfRecords = records;
        DeletedNumberOfSpiralAbysses = spiralAbyss;
        RemovedNumberOfRedisKeys = redis;
    }
}