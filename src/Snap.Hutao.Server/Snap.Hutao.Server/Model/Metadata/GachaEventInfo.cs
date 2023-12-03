// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Metadata;

[Table("gacha_events")]
[PrimaryKey(nameof(Version), nameof(Name), nameof(Locale), nameof(Order))]
public sealed class GachaEventInfo
{
    public string Version { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Locale { get; set; } = default!;

    public uint Order { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime From { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime To { get; set; }

    public GachaConfigType Type { get; set; }
}