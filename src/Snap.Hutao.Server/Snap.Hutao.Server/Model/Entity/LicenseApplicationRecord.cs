// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Model.Entity;

[Table("license_application_records")]
public sealed class LicenseApplicationRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    public int UserId { get; set; } = default!;

    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    public string ProjectUrl { get; set; } = default!;

    public string ApprovalCode { get; set; } = default!;

    public bool IsApproved { get; set; }
}