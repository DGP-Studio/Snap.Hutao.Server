// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Metadata;

[Table("known_items")]
public sealed class KnownItem
{
    [Key]
    public uint Id { get; set; }

    public uint Quality { get; set; }
}