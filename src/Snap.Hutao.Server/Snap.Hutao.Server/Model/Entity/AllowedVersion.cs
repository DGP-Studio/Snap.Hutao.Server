// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity;

[Table("allowed_versions")]
public class AllowedVersion
{
    [Key]
    public string Header { get; set; } = default!;
}