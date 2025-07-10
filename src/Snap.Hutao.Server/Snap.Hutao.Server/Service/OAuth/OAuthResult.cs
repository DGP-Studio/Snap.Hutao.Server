// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.OAuth;

public sealed class OAuthResult
{
    public bool IsSuccess { get; set; }

    public string? Error { get; set; }

    public bool IsBind { get; set; }

    [MemberNotNullWhen(false, nameof(IsBind))]
    public TokenResponse? TokenResponse { get; set; }

    public static OAuthResult Fail(string error)
    {
        return new OAuthResult
        {
            IsSuccess = false,
            Error = error,
        };
    }

    public static OAuthResult BindSuccess()
    {
        return new OAuthResult()
        {
            IsSuccess = true,
            IsBind = true,
        };
    }

    public static OAuthResult LoginSuccess(TokenResponse tokenResponse)
    {
        return new OAuthResult()
        {
            IsSuccess = true,
            TokenResponse = tokenResponse,
        };
    }
}