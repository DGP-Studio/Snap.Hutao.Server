// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Passport;

[Table("passport_verifications")]
public class PassportVerification
{
    [Key]
    public string NormalizedUserName { get; set; } = default!;

    public string VerifyCode { get; set; } = default!;

    public long GeneratedTimestamp { get; set; } = default!;

    public long ExpireTimestamp { get; set; } = default!;
}