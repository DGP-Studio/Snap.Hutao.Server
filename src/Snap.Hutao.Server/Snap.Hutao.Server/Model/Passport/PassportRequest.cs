// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public sealed class PassportRequest
{
    public string UserName { get; set; } = default!;

    public string NewUserName { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string VerifyCode { get; set; } = default!;

    public string NewVerifyCode { get; set; } = default!;

    public bool IsResetPassword { get; set; }

    public bool IsResetUsername { get; set; }

    public bool IsResetUsernameNew { get; set; }

    public bool IsCancelRegistration { get; set; }
}
