// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Service.OAuth;

namespace Snap.Hutao.Server.Model.Entity.Passport;

public class OAuthBindIdentity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    public OAuthProviderKind ProviderKind { get; set; }

    public string ProviderId { get; set; } = default!;

    public string DisplayName { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;

    public long CreatedAt { get; set; }

    public long ExpiresAt { get; set; }
}