// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class LoggedInDeviceInfo
{
    public string DeviceId { get; set; } = default!;

    public string? DeviceName { get; set; }

    public string? OperatingSystem { get; set; }

    public DateTimeOffset LastLoginAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsCurrentDevice { get; set; }
}