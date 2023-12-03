// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Authorization;

public class PassportResult
{
    public PassportResult(bool success, string message, string? key, string? token = null)
    {
        Success = success;
        Message = message;
        LocalizationKey = key;
        Token = token;
    }

    [MemberNotNullWhen(true, nameof(Token))]
    public bool Success { get; set; }

    public string Message { get; set; }

    public string? Token { get; set; }

    public string? LocalizationKey { get; set; }
}