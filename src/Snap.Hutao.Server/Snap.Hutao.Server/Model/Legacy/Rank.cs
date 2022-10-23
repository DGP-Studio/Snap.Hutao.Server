// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 造成伤害与造成伤害排行
/// </summary>
public class Rank
{
    /// <summary>
    /// 构造一个新的伤害与造成伤害排行
    /// </summary>
    /// <param name="damage">造成伤害</param>
    /// <param name="takeDamage">受到伤害</param>
    public Rank(ItemRate<int, double>? damage, ItemRate<int, double>? takeDamage)
    {
        Damage = damage;
        TakeDamage = takeDamage;
    }

    /// <summary>
    /// 造成伤害
    /// </summary>
    public ItemRate<int, double>? Damage { get; set; } = default!;

    /// <summary>
    /// 受到伤害
    /// </summary>
    public ItemRate<int, double>? TakeDamage { get; set; } = default!;
}