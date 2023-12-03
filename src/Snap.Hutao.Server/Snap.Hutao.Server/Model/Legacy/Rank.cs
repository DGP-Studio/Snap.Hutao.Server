// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class Rank
{
    public Rank(RankValue? damage, RankValue? takeDamage)
    {
        Damage = damage;
        TakeDamage = takeDamage;
    }

    public RankValue? Damage { get; set; } = default!;

    public RankValue? TakeDamage { get; set; } = default!;
}