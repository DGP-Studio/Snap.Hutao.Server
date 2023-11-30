// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

public class RankValue : ItemRate<int, double>
{
    public RankValue(int item, int value, double rate, double rateOnAvatar)
        : base(item, rate)
    {
        Value = value;
        RateOnAvatar = rateOnAvatar;
    }

    public int Value { get; set; }

    public double RateOnAvatar { get; set; }
}