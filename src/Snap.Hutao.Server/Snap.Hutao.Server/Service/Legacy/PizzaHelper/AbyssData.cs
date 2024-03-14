// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.PizzaHelper;

internal sealed class AbyssData
{
    [JsonPropertyName("submitId")]
    public Guid SubmitId { get; } = Guid.NewGuid();

    [JsonPropertyName("uid")]
    public string Uid { get; set; } = default!;

    [JsonPropertyName("submitTime")]
    public long SubmitTime { get; } = DateTimeOffset.Now.ToUnixTimeSeconds();

    [JsonPropertyName("abyssSeason")]
    public string AbyssSeason { get; set; } = default!;

    [JsonPropertyName("server")]
    public string Server { get; set; } = default!;

    [JsonPropertyName("submitDetails")]
    public List<SubmitDetailModel> SubmitDetails { get; set; } = default!;

    [JsonPropertyName("abyssRank")]
    public AbyssRankModel? AbyssRank { get; set; }

    [JsonPropertyName("owningChars")]
    public List<int> OwningChars { get; set; } = default!;

    [JsonPropertyName("battleCount")]
    public int BattleCount { get; set; }

    [JsonPropertyName("winCount")]
    public int WinCount { get; set; }
}