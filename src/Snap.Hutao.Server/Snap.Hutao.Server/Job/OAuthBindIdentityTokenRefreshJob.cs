// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Sentry;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.OAuth;
using System.Collections.Immutable;

namespace Snap.Hutao.Server.Job;

public sealed class OAuthBindIdentityTokenRefreshJob : IJob
{
    private readonly IServiceProvider serviceProvider;
    private readonly DiscordService discordService;
    private readonly AppDbContext appDbContext;

    public OAuthBindIdentityTokenRefreshJob(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.discordService = serviceProvider.GetRequiredService<DiscordService>();
        this.appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        SentryId id = SentrySdk.CaptureCheckIn("OAuthBindIdentityTokenRefreshJob", CheckInStatus.InProgress);
        try
        {
            int successCount = 0;
            int failureCount = 0;
            ImmutableArray<OAuthBindIdentity> expiringBindIdentities = this.appDbContext.OAuthBindIdentities.Where(b => b.ExpiresAt < DateTimeOffset.Now.AddDays(7).ToUnixTimeSeconds()).ToImmutableArray();
            foreach (OAuthBindIdentity identity in expiringBindIdentities)
            {
                IOAuthProvider provider = this.serviceProvider.GetRequiredKeyedService<IOAuthProvider>(identity.ProviderKind);
                if (await provider.RefreshTokenAsync(identity).ConfigureAwait(false))
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }

            await this.discordService.ReportOAuthBindIdentityTokenRefreshResultAsync(successCount, failureCount).ConfigureAwait(false);
            SentrySdk.CaptureCheckIn("OAuthBindIdentityTokenRefreshJob", CheckInStatus.Ok, id);
        }
        catch
        {
            SentrySdk.CaptureCheckIn("OAuthBindIdentityTokenRefreshJob", CheckInStatus.Error, id);
            throw;
        }
    }
}