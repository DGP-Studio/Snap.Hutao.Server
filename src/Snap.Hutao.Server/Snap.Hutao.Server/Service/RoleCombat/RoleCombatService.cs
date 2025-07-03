// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.RoleCombat;
using Snap.Hutao.Server.Model.RoleCombat;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Legacy.Primitive;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.RoleCombat;

public sealed class RoleCombatService
{
    public const string Working = "RoleCombatService.Working";

    private static readonly Func<AppDbContext, IEnumerable<RoleCombatRecord>> RecordsQuery = EF.CompileQuery((AppDbContext context) =>
        context.RoleCombatRecords.AsNoTracking());

    private static readonly Func<AppDbContext, long, IEnumerable<RoleCombatAvatar>> AvatarsQuery = EF.CompileQuery((AppDbContext context, long recordId) =>
        context.RoleCombatAvatars.AsNoTracking().Where(g => g.RecordId == recordId).AsQueryable());

    private readonly DiscordService discordService;
    private readonly AppDbContext appDbContext;
    private readonly IMemoryCache memoryCache;

    public RoleCombatService(IServiceProvider serviceProvider)
    {
        discordService = serviceProvider.GetRequiredService<DiscordService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    }

    public async Task RunAsync()
    {
        if (memoryCache.TryGetValue(Working, out _))
        {
            return;
        }

        try
        {
            memoryCache.Set(Working, true);
            Map<uint, int> resultMap = [];

            int total = await Task.Run(() => RunCore(resultMap)).ConfigureAwait(false);
            RoleCombatStatisticsItem item = new()
            {
                ScheduleId = RoleCombatScheduleId.GetForNow(),
                RecordTotal = total,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                BackupAvatarRates = resultMap.Select(kvp => new Model.Legacy.ItemRate<uint, double>(kvp.Key, kvp.Value / (double)total)).ToList(),
            };

            int scheduleId = RoleCombatScheduleId.GetForNow();
            RoleCombatStatistics? statistics = appDbContext.RoleCombatStatistics.SingleOrDefault(s => s.ScheduleId == scheduleId);

            if (statistics == null)
            {
                statistics = new()
                {
                    ScheduleId = scheduleId,
                };
                appDbContext.RoleCombatStatistics.Add(statistics);
            }

            memoryCache.Set($"RoleCombatStatistics:{scheduleId}", item);
            statistics.Data = JsonSerializer.Serialize(item);

            await appDbContext.SaveChangesAsync().ConfigureAwait(false);

            await discordService.ReportRoleCombatStatisticsAsync(item).ConfigureAwait(false);
        }
        finally
        {
            memoryCache.Remove(Working);
        }
    }

    private int RunCore(Map<uint, int> resultMap)
    {
        int count = 0;
        List<RoleCombatRecord> reocrds = RecordsQuery(appDbContext).ToList();
        foreach (ref readonly RoleCombatRecord record in CollectionsMarshal.AsSpan(reocrds))
        {
            count++;
            List<RoleCombatAvatar> avatars = AvatarsQuery(appDbContext, record.PrimaryId).ToList();

            foreach (ref readonly RoleCombatAvatar avatar in CollectionsMarshal.AsSpan(avatars))
            {
                resultMap.IncreaseOne(avatar.AvatarId);
            }
        }

        return count;
    }
}