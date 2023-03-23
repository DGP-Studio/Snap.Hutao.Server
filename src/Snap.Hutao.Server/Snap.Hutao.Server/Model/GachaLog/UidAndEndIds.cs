// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.GachaLog;

/// <summary>
/// Uid 与 Id
/// </summary>
public sealed class UidAndEndIds
{
    /// <summary>
    /// Uid
    /// </summary>
    public string Uid { get; set; } = default!;

    /// <summary>
    /// Id
    /// </summary>
    public EndIds EndIds { get; set; } = default!;
}