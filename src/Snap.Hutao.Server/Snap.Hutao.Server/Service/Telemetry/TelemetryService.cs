// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Telemetry;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Service.Telemetry;

// Scoped
public sealed class TelemetryService
{
    private readonly AppDbContext appDbContext;

    public TelemetryService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async ValueTask<CrashLogProcessResult> ProcessCrashLogAsync(HutaoUploadLog uploadLog, string version)
    {
        if (uploadLog.Id == null || uploadLog.Time == 0)
        {
            return CrashLogProcessResult.InvalidData;
        }

        string info = uploadLog.Info;
        HutaoLog? log = await appDbContext.HutaoLogs.SingleOrDefaultAsync(log => log.Info == info).ConfigureAwait(false);

        if (log != null)
        {
            log.Count += 1;
            log.Version = version;
        }
        else
        {
            log = new HutaoLog
            {
                Info = info,
                Count = 1,
                Version = version,
            };

            appDbContext.HutaoLogs.Add(log);
        }

        HashSet<string> allowedVersions = [.. appDbContext.AllowedVersions.Select(v => v.Header)];
        if (!allowedVersions.Contains(version))
        {
            log.Resolved = true;
        }

        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        HutaoLogSingleItem singleItem = new()
        {
            LogId = log.PrimaryId,
            DeviceId = uploadLog.Id,
            Time = uploadLog.Time,
        };
        await appDbContext.HutaoLogSingleItems.AddAndSaveAsync(singleItem).ConfigureAwait(false);

        return CrashLogProcessResult.Ok;
    }

    public async ValueTask<List<HutaoLog>> GetLogsByDeviceId(string deviceId)
    {
        List<HutaoLogSingleItem> items = await appDbContext.HutaoLogSingleItems
            .AsNoTracking()
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.Time)
            .Take(3)
            .ToListAsync()
            .ConfigureAwait(false);

        List<HutaoLog> logs = items.SelectList(item =>
        {
            HutaoLog log = appDbContext.HutaoLogs.AsNoTracking().Single(x => x.PrimaryId == item.LogId);
            log.Time = item.Time;
            return log;
        });

        return logs;
    }
}