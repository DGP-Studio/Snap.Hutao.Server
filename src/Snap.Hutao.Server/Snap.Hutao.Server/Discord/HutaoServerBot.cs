// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Microsoft.Extensions.Options;
using Qmmands;
using Snap.Hutao.Server.Option;
using System.Collections.Concurrent;

namespace Snap.Hutao.Server.Discord;

public sealed class HutaoServerBot : DiscordBot
{
    private readonly ConcurrentDictionary<Snowflake, CommandToken> userCommandTokens = new();
    private readonly ulong allowedGuildId;

    public HutaoServerBot(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IServiceProvider services, DiscordClient client)
        : base(options, logger, services, client)
    {
        allowedGuildId = services.GetRequiredService<AppOptions>().Discord.AllowedGuildId;
    }

    protected override string? FormatFailureReason(IDiscordCommandContext context, Qmmands.IResult result)
    {
        if (result is ExceptionResult exceptionResult)
        {
            return Markdown.CodeBlock(exceptionResult.Exception);
        }

        return base.FormatFailureReason(context, result);
    }

    protected override async ValueTask<Qmmands.IResult> OnBeforeExecuted(IDiscordCommandContext context)
    {
        if (context.Command is null)
        {
            return Qmmands.Results.Failure("无命令");
        }

        if (context is not IDiscordApplicationCommandContext applicationCommandContext)
        {
            return Qmmands.Results.Failure("非命令消息");
        }

        bool isEphemeral = context.Command!.CustomAttributes.Any(attr => attr is EphemeralAttribute);
        await applicationCommandContext.Interaction.Response().DeferAsync(isEphemeral);

        // 非私聊 且 GuildId 不匹配
        if (context.GuildId is not null && context.GuildId != allowedGuildId)
        {
            await applicationCommandContext.Interaction.Followup()
                .SendAsync(new LocalInteractionMessageResponse().WithContent("请在指定群聊使用此命令"));
            return Qmmands.Results.Failure("群聊不匹配");
        }

        if (!userCommandTokens.TryAdd(context.AuthorId, default))
        {
            await applicationCommandContext.Interaction.Followup()
                .SendAsync(new LocalInteractionMessageResponse().WithContent("请等待上个命令完成"));
            return Qmmands.Results.Failure("上个命令尚未完成");
        }

        if (context.Command.CustomAttributes.Any(attr => attr is OwnerOnlyAttribute))
        {
            if (!await context.Bot.IsOwnerAsync(context.AuthorId))
            {
                await applicationCommandContext.Interaction.Followup()
                    .SendAsync(new LocalInteractionMessageResponse().WithContent("仅开发者可使用此功能"));
                return Qmmands.Results.Failure("开发者功能");
            }
        }

        return await base.OnBeforeExecuted(context);
    }

    protected override ValueTask<bool> OnAfterExecuted(IDiscordCommandContext context, Qmmands.IResult result)
    {
        userCommandTokens.TryRemove(context.AuthorId, out _);
        return base.OnAfterExecuted(context, result);
    }

    private readonly struct CommandToken
    {
    }
}