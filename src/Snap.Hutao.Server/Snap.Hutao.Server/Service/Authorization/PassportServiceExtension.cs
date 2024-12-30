// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.Authorization;

public static class PassportServiceExtension
{
    public static string DecryptNormalizedUserName(this PassportService passportService, PassportRequest request, out string plainUserName)
    {
        plainUserName = passportService.Decrypt(request.UserName);
        return plainUserName.ToUpperInvariant();
    }

    public static string DecryptNormalizedUserNameAndVerifyCode(this PassportService passportService, PassportRequest request, out string plainUserName, out string verifyCode)
    {
        plainUserName = passportService.Decrypt(request.UserName);
        verifyCode = passportService.Decrypt(request.VerifyCode);
        return plainUserName.ToUpperInvariant();
    }

    public static string DecryptNewNormalizedUserNameAndNewVerifyCode(this PassportService passportService, PassportRequest request, out string plainUserName, out string verifyCode)
    {
        plainUserName = passportService.Decrypt(request.NewUserName);
        verifyCode = passportService.Decrypt(request.NewVerifyCode);
        return plainUserName.ToUpperInvariant();
    }
}