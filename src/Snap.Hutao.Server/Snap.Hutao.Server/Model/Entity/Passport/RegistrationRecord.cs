// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Passport;

[Table("registration_records")]
public sealed class RegistrationRecord
{
    [Key]
    public string UserName { get; set; } = default!;
}