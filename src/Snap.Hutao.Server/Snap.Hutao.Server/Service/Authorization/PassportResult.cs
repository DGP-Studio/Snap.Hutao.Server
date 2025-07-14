// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.Authorization;

public class PassportResult
{
    public PassportResult(bool success, string message, string? key, TokenResponse? token = null)
    {
        Success = success;
        Message = message;
        LocalizationKey = key;
        Token = token;
    }

    [MemberNotNullWhen(true, nameof(Token))]
    public bool Success { get; set; }

    public string Message { get; set; }

    public TokenResponse? Token { get; set; }

    public string? LocalizationKey { get; set; }
}