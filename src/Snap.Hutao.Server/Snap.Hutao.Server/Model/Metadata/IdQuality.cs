// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel;

namespace Snap.Hutao.Server.Model.Metadata;

[Obsolete("Use KnownItem instead")]
internal sealed class IdQuality
{
    public int Id { get; set; }

    public int Quality { get; set; }

    // Adapter property for weapon
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int RankLevel { get => Quality; set => Quality = value; }
}