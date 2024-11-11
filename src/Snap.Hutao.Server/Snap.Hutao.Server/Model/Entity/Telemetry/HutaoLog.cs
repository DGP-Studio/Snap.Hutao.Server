// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Telemetry;

[Table("hutao_logs")]
public class HutaoLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public string Info { get; set; } = default!;

    public int Count { get; set; } = default!;

    public bool Resolved { get; set; }

    public string Version { get; set; } = default!;

    [NotMapped]
    public long Time { get; set; }
}