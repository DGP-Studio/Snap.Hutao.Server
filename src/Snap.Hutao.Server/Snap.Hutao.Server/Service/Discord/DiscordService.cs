// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Rest;
using Snap.Hutao.Server.Discord;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.Discord;

public sealed class DiscordService
{
    // TODO: move to options
    private const string AfdianOrderIcon = "https://static.afdiancdn.com/static/img/logo/logo.png";
    private const string SpiralAbyssIcon = "https://homa.snapgenshin.com/img/SpiralAbyss.png";
    private const string GachaLogIcon = "https://homa.snapgenshin.com/img/GachaLog.png";

    private readonly HutaoServerBot hutaoServerBot;
    private readonly DiscordOptions discordOptions;

    public DiscordService(IServiceProvider serviceProvider)
    {
        hutaoServerBot = serviceProvider.GetRequiredService<HutaoServerBot>();
        discordOptions = serviceProvider.GetRequiredService<AppOptions>().Discord;
    }

    public async ValueTask ReportAfdianOrderAsync(AfdianOrderInformation info)
    {
        LocalEmbed embed = CreateStandardEmbed("Afdian Order", AfdianOrderIcon);

        embed.WithDescription($"Status: {info.Status}");

        embed.AddField("SkuId", info.SkuId);
        embed.AddField("Order", info.OrderNumber);
        embed.AddField("Count", $"× {info.OrderCount}");
        embed.AddField("Email", info.UserName);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PrivateReport, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportSpiralAbyssCleanResultAsync(SpiralAbyssRecordCleanResult result)
    {
        LocalEmbed embed = CreateStandardEmbed("Spiral Abyss Record Cleanup", SpiralAbyssIcon);

        embed.WithDescription($"In this cleanup, we cleanned:");

        embed.AddField("Records", result.DeletedNumberOfRecords);
        embed.AddField("SpiralAbyss", result.DeletedNumberOfSpiralAbysses);
        embed.AddField("RedisKeys", result.RemovedNumberOfRedisKeys);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportGachaEventStatisticsAsync(GachaEventStatistics statistics)
    {
        LocalEmbed embed = CreateStandardEmbed("Gacha Event Statistics", GachaLogIcon);

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
        LocalEmbed embed = CreateStandardEmbed("Spiral Abyss Statistics", SpiralAbyssIcon);

        embed.WithDescription("Statistics run completed");

        embed.AddField("Schedule Id", overview.ScheduleId);
        embed.AddField("Record Total", overview.RecordTotal);
        embed.AddField("SpiralAbyss Total", overview.SpiralAbyssTotal);
        embed.AddField("SpiralAbyss Full Star", overview.SpiralAbyssFullStar);
        embed.AddField("SpiralAbyss Passed", overview.SpiralAbyssPassed);
        embed.AddField("SpiralAbyss Star Average", overview.SpiralAbyssStarTotal / (double)overview.SpiralAbyssTotal);
        embed.AddField("SpiralAbyss Battle Average", overview.SpiralAbyssBattleTotal / (double)overview.SpiralAbyssTotal);
        embed.AddField("Calc Time per Record", overview.TimeAverage);

        await hutaoServerBot.SendMessageAsync(discordOptions.KnownChannels.PublicStatus, new LocalMessage().WithEmbeds(embed));
    }

    private static LocalEmbed CreateStandardEmbed(string title, string icon)
    {
        string footer = $"DGP Studio | {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
        return new LocalEmbed().WithAuthor(title, icon).WithFooter(footer);
    }
}