// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Legacy;

/// <summary>
/// 总览数据
/// </summary>
public class Overview
{
    /// <summary>
    /// 规划Id
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int RecordTotal { get; set; }

    /// <summary>
    /// 总深渊计数
    /// </summary>
    public int SpiralAbyssTotal { get; set; }

    /// <summary>
    /// 满星数
    /// </summary>
    public int SpiralAbyssFullStar { get; set; }

    /// <summary>
    /// 通关玩家总数
    /// </summary>
    public int SpiralAbyssPassed { get; set; }

    /// <summary>
    /// 总星数
    /// </summary>
    public int SpiralAbyssStarTotal { get; set; }

    /// <summary>
    /// 总战斗次数
    /// </summary>
    public long SpiralAbyssBattleTotal { get; set; }

    /// <summary>
    /// 计算完成时间
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 计算每条数据的总耗时
    /// </summary>
    public double TimeTotal { get; set; }

    /// <summary>
    /// 计算每条数据的平均耗时
    /// </summary>
    public double TimeAverage { get; set; }
}