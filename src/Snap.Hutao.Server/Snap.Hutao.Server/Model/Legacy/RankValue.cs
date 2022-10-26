﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 伤害值
/// </summary>
public class RankValue : ItemRate<int, double>
{
    /// <summary>
    /// 构造一个新的伤害值
    /// </summary>
    /// <param name="item">物品</param>
    /// <param name="value">伤害</param>
    /// <param name="rate">率</param>
    /// <param name="rateOnAvatar">角色率</param>
    public RankValue(int item, int value, double rate, double rateOnAvatar)
        : base(item, rate)
    {
        Value = value;
        RateOnAvatar = rateOnAvatar;
    }

    /// <summary>
    /// 伤害值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 角色比率
    /// </summary>
    public double RateOnAvatar { get; set; }
}