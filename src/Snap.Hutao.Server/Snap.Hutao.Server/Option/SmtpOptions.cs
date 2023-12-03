// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Option;

public sealed class SmtpOptions
{
    public string DiagnosticEmailAddress { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Server { get; set; } = default!;

    public string UserName { get; set; } = default!;
}