// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

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

    public string DeviceId { get; set; } = default!;

    public string? DeviceName { get; set; }

    public string? OperatingSystem { get; set; }

    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;
}