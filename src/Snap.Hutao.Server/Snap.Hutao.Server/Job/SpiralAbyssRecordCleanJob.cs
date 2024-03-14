// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Ranking;

namespace Snap.Hutao.Server.Job;

public class SpiralAbyssRecordCleanJob : IJob
{
    private static readonly TimeSpan LongestDaysAllowed = new(30, 0, 0, 0);

    private readonly AppDbContext appDbContext;
    private readonly IRankService rankService;
    private readonly DiscordService discordService;

    public SpiralAbyssRecordCleanJob(IServiceProvider serviceProvider)
    {
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        rankService = serviceProvider.GetRequiredService<IRankService>();
        discordService = serviceProvider.GetRequiredService<DiscordService>();
    }

    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        DateTimeOffset lastAllowed = DateTimeOffset.Now - LongestDaysAllowed;
        long lastAllowedTimestamp = lastAllowed.ToUnixTimeSeconds();

        // 批量删除 长期未提交的记录
        int deletedRecordsCount = await appDbContext.Records
            .Where(r => r.UploadTime < lastAllowedTimestamp)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        // 批量删除 深渊记录
        int deletedSpiralCount = await appDbContext.SpiralAbysses
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        long removedKeys = await rankService.ClearRanksAsync().ConfigureAwait(false);

        SpiralAbyssRecordCleanResult result = new(deletedRecordsCount, deletedSpiralCount, removedKeys);
        await discordService.ReportSpiralAbyssCleanResultAsync(result).ConfigureAwait(false);
    }
}