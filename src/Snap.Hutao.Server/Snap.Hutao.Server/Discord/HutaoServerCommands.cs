// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Bot.Commands.Application;
using Qmmands;
using Snap.Hutao.Server.Service.GachaLog.Statistics;

namespace Snap.Hutao.Server.Discord;

public sealed class HutaoServerCommands : DiscordApplicationModuleBase
{
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
}