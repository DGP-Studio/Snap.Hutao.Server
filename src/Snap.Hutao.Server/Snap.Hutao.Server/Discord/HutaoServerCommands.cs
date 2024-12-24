// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Bot.Commands.Application;
using Qmmands;
using Snap.Hutao.Server.Service.Expire;
using Snap.Hutao.Server.Service.GachaLog.Statistics;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.RoleCombat;

namespace Snap.Hutao.Server.Discord;

public sealed class HutaoServerCommands : DiscordApplicationModuleBase
{
    [OwnerOnly]
    [SlashCommand("run-statistics-spiralabyss")]
    [Description("运行深渊记录统计")]
    public async ValueTask<Qmmands.IResult> RunSpiralAbyssStatisticsAsync()
    {
        await Context.Services.GetRequiredService<StatisticsService>().RunAsync().ConfigureAwait(false);
        LocalEmbed embed = Embed.CreateStandardEmbed("深渊记录统计", Embed.GachaLogIcon);
        embed.WithDescription("深渊记录统计完成");
        LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
            .WithEmbeds(embed);
        return Response(response);
    }

    [OwnerOnly]
    [SlashCommand("run-statistics-gachalog")]
    [Description("运行祈愿记录统计")]
    public async ValueTask<Qmmands.IResult> RunGachaLogStatisticsAsync()
    {
        await Context.Services.GetRequiredService<GachaLogStatisticsService>().RunAsync().ConfigureAwait(false);
        LocalEmbed embed = Embed.CreateStandardEmbed("祈愿记录统计", Embed.GachaLogIcon);
        embed.WithDescription("祈愿记录统计完成");
        LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
            .WithEmbeds(embed);
        return Response(response);
    }

    [OwnerOnly]
    [SlashCommand("run-statistics-rolecombat")]
    [Description("运行剧演记录统计")]
    public async ValueTask<Qmmands.IResult> RunRoleCombatStatisticsAsync()
    {
        await Context.Services.GetRequiredService<RoleCombatService>().RunAsync().ConfigureAwait(false);
        LocalEmbed embed = Embed.CreateStandardEmbed("幻想真境剧诗统计", Embed.GachaLogIcon);
        embed.WithDescription("幻想真境剧诗完成");
        LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
            .WithEmbeds(embed);
        return Response(response);
    }

    [OwnerOnly]
    [SlashCommand("extend-gachalog-all")]
    [Description("延长祈愿记录时间")]
    public async ValueTask<Qmmands.IResult> ExtendGachaLogTermForAllAsync([Description("延长天数")] int days)
    {
        await Context.Services.GetRequiredService<GachaLogExpireService>().ExtendTermForAllUsersAsync(days).ConfigureAwait(false);
        LocalEmbed embed = Embed.CreateStandardEmbed("祈愿记录统计", Embed.GachaLogIcon);
        embed.WithDescription("祈愿记录时间已延长");
        LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
            .WithEmbeds(embed);
        return Response(response);
    }

    [OwnerOnly]
    [SlashCommand("extend-gachalog-one")]
    [Description("延长祈愿记录时间")]
    public async ValueTask<Qmmands.IResult> ExtendGachaLogTermAsync([Description("邮箱")] string email, [Description("延长天数")] int days)
    {
        await Context.Services.GetRequiredService<GachaLogExpireService>().ExtendTermForUserNameAsync(email, days).ConfigureAwait(false);
        LocalEmbed embed = Embed.CreateStandardEmbed("祈愿记录统计", Embed.GachaLogIcon);
        embed.WithDescription("祈愿记录时间已延长");
        LocalInteractionMessageResponse response = new LocalInteractionMessageResponse()
            .WithEmbeds(embed);
        return Response(response);
    }
}