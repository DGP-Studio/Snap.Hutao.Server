// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Rest;
using Snap.Hutao.Server.Discord;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.RoleCombat;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Afdian;
using Snap.Hutao.Server.Service.Github;

namespace Snap.Hutao.Server.Service.Discord;

// Singleton
public sealed class DiscordService
{
    private readonly HutaoServerBot hutaoServerBot;
    private readonly DiscordOptions discordOptions;

    public DiscordService(IServiceProvider serviceProvider)
    {
        hutaoServerBot = serviceProvider.GetRequiredService<HutaoServerBot>();
        discordOptions = serviceProvider.GetRequiredService<AppOptions>().Discord;
    }

    public async ValueTask ReportAfdianOrderAsync(AfdianOrderInformation info)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Afdian Order", Embed.AfdianOrderIcon);

        embed.WithDescription($"Status: {info.Status}");

        embed.AddField("SkuId", info.SkuId);
        embed.AddField("Order", info.OrderNumber);
        embed.AddField("Count", $"× {info.OrderCount}");
        embed.AddField("Email", info.UserName);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PrivateReport, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportSpiralAbyssCleanResultAsync(SpiralAbyssRecordCleanResult result)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Spiral Abyss Record Cleanup", Embed.SpiralAbyssIcon);

        embed.WithDescription($"In this cleanup, we cleanned:");

        embed.AddField("Records", result.DeletedNumberOfRecords);
        embed.AddField("SpiralAbyss", result.DeletedNumberOfSpiralAbysses);
        embed.AddField("RedisKeys", result.RemovedNumberOfRedisKeys);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportRoleCombatCleanResultAsync(RoleCombatRecordCleanResult result)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Role Combat Record Cleanup", Embed.SpiralAbyssIcon);

        embed.WithDescription($"In this cleanup, we cleanned:");

        embed.AddField("Records", result.DeletedNumberOfRecords);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportGachaEventStatisticsAsync(GachaEventStatistics statistics)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Gacha Event Statistics", Embed.GachaLogIcon);

        embed.WithDescription($"Status: {statistics.Status}");

        if (statistics.InvalidUids is { } set)
        {
            foreach (string uid in set)
            {
                embed.AddField("Invalid Uid", uid, true);
            }
        }
        else
        {
            embed.AddField("Pulls Enumerated", statistics.PullsEnumerated, true);
        }

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportSpiralAbyssStatisticsAsync(Overview overview)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Spiral Abyss Statistics", Embed.SpiralAbyssIcon);

        embed.WithDescription("Statistics run completed");

        embed.AddField("Schedule Id", overview.ScheduleId, true);
        embed.AddField("Record Total", overview.RecordTotal, true);
        embed.AddField("SpiralAbyss Total", overview.SpiralAbyssTotal, true);
        embed.AddField("SpiralAbyss Full Star", overview.SpiralAbyssFullStar, true);
        embed.AddField("SpiralAbyss Passed", overview.SpiralAbyssPassed, true);
        embed.AddField("SpiralAbyss Star Average", overview.SpiralAbyssStarTotal / (double)overview.SpiralAbyssTotal, true);
        embed.AddField("SpiralAbyss Battle Average", overview.SpiralAbyssBattleTotal / (double)overview.SpiralAbyssTotal, true);
        embed.AddField("Calc Time per Record", overview.TimeAverage, true);

        await hutaoServerBot
            .SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed))
            .WaitAsync(new CancellationToken(true))
            .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public async ValueTask ReportRoleCombatStatisticsAsync(RoleCombatStatisticsItem statistics)
    {
        LocalEmbed embed = Embed.CreateStandardEmbed("Role Combat Statistics", Embed.GachaLogIcon);

        embed.WithDescription("Statistics run completed");

        embed.AddField("Record Total", statistics.RecordTotal, true);

        await hutaoServerBot
            .SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed))
            .WaitAsync(new CancellationToken(true))
            .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    public async ValueTask ReportGithubWebhookAsync(GithubWebhookResult githubMessage)
    {
        ulong channelId = githubMessage.Event switch
        {
            GithubWebhookEvent.WorkflowRun => discordOptions.KnownChannels.Alpha,
            GithubWebhookEvent.Release => discordOptions.KnownChannels.Announcement,
            _ => throw new NotSupportedException(),
        };

        LocalMessage message = new LocalMessage()
            .WithContent(githubMessage.MarkdownBody);

        await hutaoServerBot.SendMessageAsync(channelId, message);
    }
}