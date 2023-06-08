// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Controller.Helper;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.OpenSource;
using Snap.Hutao.Server.Model.ReCaptcha;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.ReCaptcha;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 开源许可证控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(RequestFilter))]
[ApiExplorerSettings(IgnoreApi = true)]
public class AccessionController : ControllerBase
{
    private readonly ReCaptchaService reCaptchaService;
    private readonly AppDbContext appDbContext;
    private readonly MailService mailService;
    private readonly UserManager<HutaoUser> userManager;

    /// <summary>
    /// 构造一个新的开源许可证控制器
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public AccessionController(IServiceProvider serviceProvider)
    {
        reCaptchaService = serviceProvider.GetRequiredService<ReCaptchaService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        mailService = serviceProvider.GetRequiredService<MailService>();
        userManager = serviceProvider.GetRequiredService<UserManager<HutaoUser>>();
    }

    /// <summary>
    /// 申请开发者许可
    /// </summary>
    /// <param name="info">申请信息</param>
    /// <returns>任务</returns>
    [HttpPost("ApplyOpenSourceLicense")]
    public async Task<IActionResult> ApplyOpenSourceLicenseAsync([FromBody] LicenseApplication info)
    {
        ReCaptchaResponse? response = await reCaptchaService.VerifyAsync(info.Token).ConfigureAwait(false);

        if (response != null && response.Success && response.Action == "ApplyOpenSourceLicense" && response.Score > 0.5f)
        {
            if (await userManager.FindByNameAsync(info.UserName).ConfigureAwait(false) is HutaoUser user)
            {
                if (!await appDbContext.Licenses.AnyAsync(l => l.UserId == user.Id).ConfigureAwait(false))
                {
                    string code = RandomHelper.GetRandomStringWithChars(32);
                    LicenseApplicationRecord record = new()
                    {
                        UserId = user.Id,
                        ProjectUrl = info.ProjectUrl,
                        ApprovalCode = code,
                    };
                    await appDbContext.Licenses.AddAndSaveAsync(record).ConfigureAwait(false);

                    await mailService.SendDiagnosticOpenSourceLicenseNotificationAsync(info.UserName, info.ProjectUrl, code).ConfigureAwait(false);
                    return Model.Response.Response.Success("申请成功，请耐心等待");
                }
                else
                {
                    return Model.Response.Response.Fail(ReturnCode.AlreadyAppliedForLicense, "申请成功，请勿重复申请");
                }
            }
            else
            {
                return Model.Response.Response.Fail(ReturnCode.RegisterFail, "申请失败，账户不存在");
            }
        }
        else
        {
            return Model.Response.Response.Fail(ReturnCode.ReCaptchaVerificationFailed, "申请失败，reCAPTCHA 验证失败");
        }
    }

    /// <summary>
    /// 批准开发者许可
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="code">批准代码</param>
    /// <returns>任务</returns>
    [HttpGet("ApproveOpenSourceLicense")]
    public async Task<IActionResult> ApproveOpenSourceLicenseAsync([FromQuery] string userName, [FromQuery] string code)
    {
        if (await userManager.FindByNameAsync(userName).ConfigureAwait(false) is HutaoUser user)
        {
            if (await appDbContext.Licenses.SingleOrDefaultAsync(l => l.UserId == user.Id && l.ApprovalCode == code).ConfigureAwait(false) is LicenseApplicationRecord record)
            {
                record.IsApproved = true;
                await appDbContext.Licenses.UpdateAndSaveAsync(record).ConfigureAwait(false);

                user.IsLicensedDeveloper = true;
                await userManager.UpdateAsync(user).ConfigureAwait(false);

                await mailService.SendOpenSourceLicenseNotificationApprovalAsync(userName).ConfigureAwait(false);
                return Model.Response.Response.Success("批准成功");
            }
        }

        return Model.Response.Response.Fail(ReturnCode.LicenseApprovalFailed, "批准失败");
    }
}