// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Passport;

namespace Snap.Hutao.Server.Service.OAuth;

public sealed class OAuthBindState
{
    public OAuthBindState(string deviceId, string? deviceName, string? deviceOs, string callbackUri)
    {
        DeviceInfo = new(deviceId, deviceName, deviceOs);
        CallbackUri = callbackUri;
    }

    public OAuthBindState(int userId, string deviceId, string? deviceName, string? deviceOs, string callbackUri)
    {
        UserId = userId;
        DeviceInfo = new(deviceId, deviceName, deviceOs);
        CallbackUri = callbackUri;
    }

    public int UserId { get; set; } = -1;

    public DeviceInfo DeviceInfo { get; set; }

    public string CallbackUri { get; set; }
}