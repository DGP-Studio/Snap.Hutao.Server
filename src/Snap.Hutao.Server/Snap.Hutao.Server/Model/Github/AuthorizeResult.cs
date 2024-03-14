// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Model.Github;

public sealed class AuthorizeResult
{
    [MemberNotNullWhen(true, nameof(Token))]
    public bool Success { get; set; }

    public ReturnCode ReturnCode { get; set; } = ReturnCode.Success;

    public string? Token { get; set; }
}