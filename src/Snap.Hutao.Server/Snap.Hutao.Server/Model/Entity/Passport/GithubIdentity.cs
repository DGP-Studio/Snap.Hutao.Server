// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Passport;

public sealed class GithubIdentity
{
    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    public int UserId { get; set; }

    [Key]
    public long Id { get; set; }

    public string NodeId { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;

    public long ExipresAt { get; set; }
}