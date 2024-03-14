// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.PizzaHelper;

internal sealed class AbyssRankModel
{
    [JsonPropertyName("topDamageValue")]
    public int TopDamageValue { get; set; }

    [JsonPropertyName("topDamage")]
    public int TopDamage { get; set; }

    [JsonPropertyName("topTakeDamage")]
    public int TopTakeDamage { get; set; }

    [JsonPropertyName("topDefeat")]
    public int TopDefeat { get; set; }

    [JsonPropertyName("topEUsed")]
    public int TopEUsed { get; set; }

    [JsonPropertyName("topQUsed")]
    public int TopQUsed { get; set; }
}
