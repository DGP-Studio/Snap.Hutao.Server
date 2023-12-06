// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;

namespace Snap.Hutao.Server.Discord;

public static class Embed
{
    public const string AfdianOrderIcon = "https://static.afdiancdn.com/static/img/logo/logo.png";
    public const string GachaLogIcon = "https://homa.snapgenshin.com/img/GachaLog.png";
    public const string SpiralAbyssIcon = "https://homa.snapgenshin.com/img/SpiralAbyss.png";

    public static LocalEmbed CreateStandardEmbed(string title, string icon)
    {
        string footer = $"DGP Studio | {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}";
        return new LocalEmbed().WithAuthor(title, icon).WithFooter(footer);
    }
}