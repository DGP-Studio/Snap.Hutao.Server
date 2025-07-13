// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Model.Entity.Passport;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string Token { get; set; } = default!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DeviceInfo DeviceInfo { get; set; } = default!;

    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;
}