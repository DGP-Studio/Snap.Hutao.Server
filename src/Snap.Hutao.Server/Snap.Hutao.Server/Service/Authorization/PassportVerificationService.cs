// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;

namespace Snap.Hutao.Server.Service.Authorization;

// Scoped
public sealed class PassportVerificationService
{
    private const int ExpireThresholdSeconds = 15 * 60;
    private const int RegenerateThresholdSeconds = 60;

    private readonly AppDbContext appDbContext;

    public PassportVerificationService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    // This method will remove verification code if new one can regenerate.
    public bool TryGetNonExpiredVerifyCode(string normalizedUserName, out string? code)
    {
        PassportVerification? verification = appDbContext.PassportVerifications
            .SingleOrDefault(x => x.NormalizedUserName == normalizedUserName);

        if (verification is null)
        {
            code = default;
            return false;
        }

        if (verification.GeneratedTimestamp + RegenerateThresholdSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            // Remove past code
            appDbContext.PassportVerifications.RemoveAndSave(verification);
            code = default;
            return false;
        }

        code = verification.VerifyCode;
        return true;
    }

    public string GenerateVerifyCodeForUserName(string normalizedUserName)
    {
        string code = RandomHelper.GetUpperAndNumberString(8);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PassportVerification verification = new()
        {
            NormalizedUserName = normalizedUserName,
            VerifyCode = code,
            GeneratedTimestamp = now,
            ExpireTimestamp = now + ExpireThresholdSeconds,
        };

        appDbContext.PassportVerifications.AddAndSave(verification);
        return code;
    }

    public void RemoveVerifyCodeForUserName(string normalizedUserName)
    {
        PassportVerification? verification = appDbContext.PassportVerifications
            .SingleOrDefault(x => x.NormalizedUserName == normalizedUserName);

        if (verification is not null)
        {
            appDbContext.PassportVerifications.RemoveAndSave(verification);
        }
    }

    public bool TryValidateVerifyCode(string normalizedUserName, string code)
    {
        PassportVerification? verification = appDbContext.PassportVerifications
            .SingleOrDefault(x => x.NormalizedUserName == normalizedUserName);

        if (verification is null)
        {
            return false;
        }

        if (verification.ExpireTimestamp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            // Expired, remove
            appDbContext.PassportVerifications.RemoveAndSave(verification);
            return false;
        }

        if (!string.Equals(verification.VerifyCode, code, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Verified, remove
        appDbContext.PassportVerifications.RemoveAndSave(verification);
        return true;
    }
}