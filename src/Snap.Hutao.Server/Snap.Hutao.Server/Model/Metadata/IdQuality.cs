// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Metadata;

/// <summary>
/// Id 与 等级
/// </summary>
internal sealed class IdQuality
{
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public int RankLevel { get => Quality; set => Quality = value; }
}