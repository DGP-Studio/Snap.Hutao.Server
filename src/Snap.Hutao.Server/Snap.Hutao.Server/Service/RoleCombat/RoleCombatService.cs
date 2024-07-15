// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

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
            Map<int, int> resultMap = [];

            int total = await Task.Run(() => RunCore(resultMap)).ConfigureAwait(false);
            RoleCombatStatistics statistics = new()
            {
                RecordTotal = total,
                BackupAvatarRates = resultMap.Select(kvp => new Model.Legacy.ItemRate<int, double>(kvp.Key, kvp.Value / (double)total)).ToList(),
            };

            RoleCombatStatistics? existed = appDbContext.GachaStatistics.SingleOrDefault(s => s.Name == name);

            if (statistics == null)
            {
                statistics = GachaStatistics.CreateWithName(name);
                appDbContext.GachaStatistics.Add(statistics);
            }

            memoryCache.Set(name, data);
            statistics.Data = JsonSerializer.Serialize(data);

            appDbContext.SaveChanges();

            await discordService.ReportRoleCombatStatisticsAsync(statistics).ConfigureAwait(false);
        }
        finally
        {
            memoryCache.Remove(Working);
        }
    }

    private int RunCore(Map<int, int> resultMap)
    {
        List<RoleCombatRecord> reocrds = RecordsQuery(appDbContext).ToList();
        foreach (ref RoleCombatRecord record in CollectionsMarshal.AsSpan(reocrds))
        {
            // 按 Id 递增
            List<RoleCombatAvatar> avatars = AvatarsQuery(appDbContext, record.PrimaryId).ToList();
            tracker.Track(gachaItems);
        }
    }
}