// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class DeviceInfo
{
    public DeviceInfo(string deviceId, string? deviceName, string? operatingSystem)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        OperatingSystem = operatingSystem;
    }

    public string DeviceId { get; set; } = default!;

    public string? DeviceName { get; set; }

    public string? OperatingSystem { get; set; }
}