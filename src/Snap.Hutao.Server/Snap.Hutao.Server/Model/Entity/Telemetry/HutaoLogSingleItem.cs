// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Telemetry;

[Table("hutao_log_items")]
public class HutaoLogSingleItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public long LogId { get; set; }

    [ForeignKey(nameof(LogId))]
    public HutaoLog Log { get; set; } = default!;

    [MaxLength(32)]
    public string DeviceId { get; set; } = default!;

    public long Time { get; set; }
}