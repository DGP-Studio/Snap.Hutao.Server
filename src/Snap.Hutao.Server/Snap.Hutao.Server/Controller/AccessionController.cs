// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.OpenSource;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Service.Licensing;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AccessionController : ControllerBase
{
    private readonly AccessionService accessionService;

    public AccessionController(AccessionService accessionService)
    {
        this.accessionService = accessionService;
    }

    [HttpPost("ApplyOpenSourceLicense")]
    public async Task<IActionResult> ApplyOpenSourceLicenseAsync([FromBody] LicenseApplication info)
    {
        return await accessionService.ProcessApplicationAsync(info).ConfigureAwait(false) switch
        {
            ApplicationProcessResult.Ok => Model.Response.Response.Success("申请成功，请耐心等待"),
            ApplicationProcessResult.ReCaptchaVerificationFailed => Model.Response.Response.Fail(ReturnCode.ReCaptchaVerifyFailed, "申请失败，reCAPTCHA 验证失败"),
            ApplicationProcessResult.UsetNotExists => Model.Response.Response.Fail(ReturnCode.UserNameNotExists, "申请失败，账户不存在"),
            ApplicationProcessResult.AlreadyApplied => Model.Response.Response.Fail(ReturnCode.AlreadyAppliedForLicense, "申请成功，请勿重复申请"),
            _ => throw new InvalidOperationException(),
        };
    }

    [HttpGet("ApproveOpenSourceLicense")]
    public async Task<IActionResult> ApproveOpenSourceLicenseAsync([FromQuery] string userName, [FromQuery] string code)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(code))
        {
            return Model.Response.Response.Fail(ReturnCode.LicenseApprovalFailed, "批准失败");
        }

        return await accessionService.ApproveApplicationAsync(userName, code).ConfigureAwait(false) switch
        {
            ApplicationApproveResult.Ok => Model.Response.Response.Success("批准成功"),
            ApplicationApproveResult.UsetNotExists => Model.Response.Response.Fail(ReturnCode.LicenseApprovalFailed, "批准失败，账户不存在"),
            ApplicationApproveResult.NoSuchApplication => Model.Response.Response.Fail(ReturnCode.LicenseApprovalFailed, "批准失败，申请不存在"),
            _ => Model.Response.Response.Fail(ReturnCode.LicenseApprovalFailed, "批准失败"),
        };
    }
}