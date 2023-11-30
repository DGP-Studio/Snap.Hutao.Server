// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Rest;
using Snap.Hutao.Server.Discord;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Afdian;

namespace Snap.Hutao.Server.Service.Discord;

public sealed class DiscordService
{
    // TODO: move to options
    private const string AfdianOrderIcon = "https://static.afdiancdn.com/static/img/logo/logo.png";
    private const string SpiralAbyssIcon = "https://homa.snapgenshin.com/img/SpiralAbyss.png";

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

        await hutaoServerBot.SendMessageAsync(discordOptions.ChannelIds.PrivateReport, new LocalMessage().WithEmbeds(embed));
    }

    public async ValueTask ReportSpiralAbyssCleanResultAsync(SpiralAbyssRecordCleanResult result)
    {
        LocalEmbed embed = CreateStandardEmbed("Spiral Abyss Record Cleanup", SpiralAbyssIcon);

        embed.WithDescription($"In this cleanup, we cleanned:");

        embed.AddField("Records", result.DeletedNumberOfRecords);
        embed.AddField("SpiralAbyss", result.DeletedNumberOfSpiralAbysses);
        embed.AddField("RedisKeys", result.RemovedNumberOfRedisKeys);

        await hutaoServerBot.SendMessageAsync(discordOptions.ChannelIds.PrivateReport, new LocalMessage().WithEmbeds(embed));
    }

    private static LocalEmbed CreateStandardEmbed(string title, string icon)
    {
        string footer = $"DGP Studio | {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
        return new LocalEmbed().WithAuthor(title, icon).WithFooter(footer);
    }
}