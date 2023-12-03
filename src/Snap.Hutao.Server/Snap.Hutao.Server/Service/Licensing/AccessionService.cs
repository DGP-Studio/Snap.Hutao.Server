// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Core;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.OpenSource;
using Snap.Hutao.Server.Model.ReCaptcha;
using Snap.Hutao.Server.Service.ReCaptcha;

namespace Snap.Hutao.Server.Service.Licensing;

// Scoped
public sealed class AccessionService
{
    private readonly ReCaptchaService reCaptchaService;
    private readonly UserManager<HutaoUser> userManager;
    private readonly AppDbContext appDbContext;
    private readonly MailService mailService;

    public AccessionService(IServiceProvider serviceProvider)
    {
        reCaptchaService = serviceProvider.GetRequiredService<ReCaptchaService>();
        userManager = serviceProvider.GetRequiredService<UserManager<HutaoUser>>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        mailService = serviceProvider.GetRequiredService<MailService>();
    }

    public async ValueTask<ApplicationProcessResult> ProcessApplicationAsync(LicenseApplication info)
    {
        ReCaptchaResponse? response = await reCaptchaService.VerifyAsync(info.Token).ConfigureAwait(false);

        if (response is not { Success: true, Action: "ApplyOpenSourceLicense", Score: > 0.5f })
        {
            return ApplicationProcessResult.ReCaptchaVerificationFailed;
        }

        if (await userManager.FindByNameAsync(info.UserName).ConfigureAwait(false) is not HutaoUser user)
        {
            return ApplicationProcessResult.UsetNotExists;
        }

        if (await appDbContext.Licenses.AnyAsync(l => l.UserId == user.Id).ConfigureAwait(false))
        {
            return ApplicationProcessResult.AlreadyApplied;
        }

        string code = RandomHelper.GetUpperAndNumberString(32);

        LicenseApplicationRecord record = new()
        {
            UserId = user.Id,
            ProjectUrl = info.ProjectUrl,
            ApprovalCode = code,
        };
        await appDbContext.Licenses.AddAndSaveAsync(record).ConfigureAwait(false);

        await mailService.SendApproveOpenSourceLicenseNotificationAsync(info.UserName, info.ProjectUrl, code).ConfigureAwait(false);
        return ApplicationProcessResult.Ok;
    }

    public async ValueTask<ApplicationApproveResult> ApproveApplicationAsync(string userName, string code)
    {
        if (await userManager.FindByNameAsync(userName).ConfigureAwait(false) is not HutaoUser user)
        {
            return ApplicationApproveResult.UsetNotExists;
        }

        LicenseApplicationRecord? record = await appDbContext.Licenses
            .SingleOrDefaultAsync(l => l.UserId == user.Id && l.ApprovalCode == code)
            .ConfigureAwait(false);

        if (record is null)
        {
            return ApplicationApproveResult.NoSuchApplication;
        }

        record.IsApproved = true;
        await appDbContext.Licenses.UpdateAndSaveAsync(record).ConfigureAwait(false);

        user.IsLicensedDeveloper = true;
        await userManager.UpdateAsync(user).ConfigureAwait(false);

        await mailService.SendOpenSourceLicenseNotificationApprovalAsync(userName).ConfigureAwait(false);
        return ApplicationApproveResult.Ok;
    }
}