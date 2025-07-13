// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Option;
using StackExchange.Redis;

namespace Snap.Hutao.Server.Service.Authorization;

// Scoped
public sealed class PassportVerificationService
{
    private const int ExpireThresholdSeconds = 15 * 60;
    private const int RegenerateThresholdSeconds = 60;

    private readonly ConnectionMultiplexer redis;

    public PassportVerificationService(AppOptions appOptions, ILogger<PassportVerificationService> logger)
    {
        string redisAddress = appOptions.RedisAddress;
        this.redis = ConnectionMultiplexer.Connect(redisAddress);
    }

    // This method will remove verification code if new one can regenerate.
    public bool TryGetNonExpiredVerifyCode(string normalizedUserName, string key, out string? code)
    {
        IDatabase db = this.redis.GetDatabase(15);

        string? redisCode = db.StringGet($"{normalizedUserName}.{key}");
        if (redisCode is null)
        {
            code = default;
            return false;
        }

        if (db.StringGet($"{normalizedUserName}.{key}.regenerate") != RedisValue.EmptyString)
        {
            db.KeyDelete($"{normalizedUserName}.{key}");
            code = default;
            return false;
        }

        code = redisCode;
        return true;
    }

    public string GenerateVerifyCode(string normalizedUserName, string key)
    {
        string code = RandomHelper.GetUpperAndNumberString(8);
        IDatabase db = this.redis.GetDatabase(15);

        db.StringSet($"{normalizedUserName}.{key}", code, TimeSpan.FromSeconds(ExpireThresholdSeconds));
        db.StringSet($"{normalizedUserName}.{key}.regenerate", RedisValue.EmptyString, TimeSpan.FromSeconds(RegenerateThresholdSeconds));
        return code;
    }

    public void RemoveVerifyCode(string normalizedUserName, string key)
    {
        IDatabase db = this.redis.GetDatabase(15);

        db.KeyDelete($"{normalizedUserName}.{key}");
        db.KeyDelete($"{normalizedUserName}.{key}.regenerate");
    }

    public bool TryValidateVerifyCode(string normalizedUserName, string key, string code)
    {
        IDatabase db = this.redis.GetDatabase(15);

        string? redisCode = db.StringGet($"{normalizedUserName}.{key}");
        if (redisCode is null)
        {
            return false;
        }

        if (!string.Equals(redisCode, code, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        db.KeyDelete($"{normalizedUserName}.{key}");
        db.KeyDelete($"{normalizedUserName}.{key}.regenerate");
        return true;
    }
}