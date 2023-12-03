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
    private const int ExpireSeconds = 120;

    private readonly AppDbContext appDbContext;

    public PassportVerificationService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public bool TryGetNonExpiredVerifyCode(string normalizedUserName, out string? code)
    {
        PassportVerification? verification = appDbContext.PassportVerifications
            .SingleOrDefault(x => x.NormalizedUserName == normalizedUserName);

        if (verification is null)
        {
            code = default;
            return false;
        }

        if (verification.ExpireTimestamp > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            // Expired, remove
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
        PassportVerification verification = new()
        {
            NormalizedUserName = normalizedUserName,
            VerifyCode = code,
            ExpireTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ExpireSeconds,
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

        if (verification.VerifyCode != code)
        {
            return false;
        }

        // Verified, remove
        appDbContext.PassportVerifications.RemoveAndSave(verification);
        return true;
    }
}