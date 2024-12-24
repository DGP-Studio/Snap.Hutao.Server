// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Passport;

public class UserInfo
{
    public string? NormalizedUserName { get; set; }

    public string? UserName { get; set; }

    public bool IsLicensedDeveloper { get; set; }

    public bool IsMaintainer { get; set; }

    public DateTimeOffset GachaLogExpireAt { get; set; }

    public DateTimeOffset CdnExpireAt { get; set; }
}