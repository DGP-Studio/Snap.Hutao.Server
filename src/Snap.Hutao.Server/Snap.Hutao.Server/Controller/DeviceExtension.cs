// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Controller;

internal static class DeviceExtension
{
    private const string HutaoDeviceIdHeader = "x-hutao-device-id";
    private const string HutaoDeviceNameHeader = "x-hutao-device-name";
    private const string HutaoDeviceOsHeader = "x-hutao-device-os";

    public static DeviceInfo GetDeviceInfo(this ControllerBase controller)
    {
        return controller.HttpContext.GetDeviceInfo();
    }

    public static DeviceInfo GetDeviceInfo(this ActionExecutingContext context)
    {
        return context.HttpContext.GetDeviceInfo();
    }

    public static DeviceInfo GetDeviceInfo(this HttpContext context)
    {
        string? deviceId = context.Request.Headers[HutaoDeviceIdHeader];
        string? deviceName = context.Request.Headers[HutaoDeviceNameHeader];
        string? deviceOs = context.Request.Headers[HutaoDeviceOsHeader];

        ArgumentNullException.ThrowIfNull(deviceId);

        return new(deviceId, deviceName, deviceOs);
    }
}