// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.OAuth;

public sealed class OAuthBindStatus
{
    public bool IsBinded { get; set; }

    [MemberNotNullWhen(true, nameof(IsBinded))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; set; } = default!;

    [MemberNotNullWhen(true, nameof(IsBinded))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? CreateTime { get; set; }

    public static OAuthBindStatus Binded(OAuthBindIdentity identity)
    {
        return new()
        {
            IsBinded = true,
            DisplayName = identity.DisplayName,
            CreateTime = identity.CreatedAt,
        };
    }

    public static OAuthBindStatus NotBinded()
    {
        return new()
        {
            IsBinded = false,
        };
    }
}