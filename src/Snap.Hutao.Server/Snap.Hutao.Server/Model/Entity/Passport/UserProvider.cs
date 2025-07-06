// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Passport;

public class UserProvider
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    // "GitHub", "Google", etc.
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string ProviderId { get; set; } = default!;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public HutaoUser User { get; set; } = default!;
}