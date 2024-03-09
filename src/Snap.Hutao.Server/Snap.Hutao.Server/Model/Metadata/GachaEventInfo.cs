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

    public uint UpOrangeItem1 { get; set; }

    public uint UpOrangeItem2 { get; set; }

    public uint UpOrangeItem3 { get; set; }

    public uint UpOrangeItem4 { get; set; }

    public uint UpOrangeItem5 { get; set; }

    public uint UpOrangeItem6 { get; set; }

    public uint UpOrangeItem7 { get; set; }

    public uint UpOrangeItem8 { get; set; }

    public uint UpOrangeItem9 { get; set; }

    public uint UpOrangeItem10 { get; set; }

    public uint UpOrangeItem11 { get; set; }

    public uint UpOrangeItem12 { get; set; }

    public uint UpOrangeItem13 { get; set; }

    public uint UpOrangeItem14 { get; set; }

    public uint UpOrangeItem15 { get; set; }

    public uint UpOrangeItem16 { get; set; }

    public uint UpOrangeItem17 { get; set; }

    public uint UpPurpleItem1 { get; set; }

    public uint UpPurpleItem2 { get; set; }

    public uint UpPurpleItem3 { get; set; }

    public uint UpPurpleItem4 { get; set; }

    public uint UpPurpleItem5 { get; set; }

    public GachaConfigType Type { get; set; }
}