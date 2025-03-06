// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Options;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Github;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Github;
using System.IdentityModel.Tokens.Jwt;

namespace Snap.Hutao.Server.Controller;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "Passport")]
public class GithubController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly GithubOptions githubOptions;
    private readonly GithubService githubService;
    private readonly IOptionsMonitor<JwtBearerOptions> jwtBearerOptions;
    private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

    public GithubController(IServiceProvider serviceProvider)
    {
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
        githubOptions = serviceProvider.GetRequiredService<AppOptions>().Github;
        githubService = serviceProvider.GetRequiredService<GithubService>();
        jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
    }

    [HttpGet("OAuth/RedirectLogin")]
    public async Task<IActionResult> RedirectLoginAsync([FromQuery(Name = "token")] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToError(ReturnCode.InvalidQueryString);
        }

        JwtBearerOptions options = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        jwtSecurityTokenHandler.ValidateToken(token, options.TokenValidationParameters, out SecurityToken validatedToken);
        if (validatedToken is not JwtSecurityToken jwtSecurityToken)
        {
            return RedirectToError(ReturnCode.LoginFail);
        }

        int userId;
        try
        {
            userId = int.Parse(jwtSecurityToken.Claims.Single(c => c.Type == PassportClaimTypes.UserId).Value);
        }
        catch
        {
            return RedirectToError(ReturnCode.UserNameNotExists);
        }

        HutaoUser? user = await appDbContext.Users.SingleOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

        if (user is null)
        {
            return RedirectToError(ReturnCode.UserNameNotExists);
        }

        string state = githubService.CreateStateForUser(user);
        return Redirect($"https://github.com/login/oauth/authorize?client_id={githubOptions.ClientId}&state={state}");
    }

    [HttpGet("OAuth/Authorize")]
    public async Task<IActionResult> HandleAuthorizationCallbackAsync([FromQuery(Name = "code")] string code, [FromQuery(Name = "state")] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            return RedirectToError(ReturnCode.GithubAuthorizationCanceled);
        }

        if (string.IsNullOrEmpty(state))
        {
            return RedirectToError(ReturnCode.InvalidQueryString);
        }

        AuthorizeResult result = await githubService.HandleAuthorizationCallbackAsync(code, state).ConfigureAwait(false);
        if (result.Success)
        {
            // Authorized
            return Redirect($"https://passport.snapgenshin.cn/api/users/login?token={result.Token}");
        }
        else
        {
            return RedirectToError(result.ReturnCode);
        }
    }

    [Authorize]
    [HttpGet("OAuth/UnAuthorize")]
    public async Task<IActionResult> UnAuthorizeAsync()
    {
        int userId = this.GetUserId();
        int count = await appDbContext.GithubIdentities.Where(g => g.UserId == userId).ExecuteDeleteAsync().ConfigureAwait(false);
        return Response<UnAuthorizeResult>.Success("操作完成", new() { Count = count });
    }

    [Authorize]
    [HttpGet("OAuth/AuthorizationStatus")]
    public async Task<IActionResult> GetAuthorizationStatusAsync()
    {
        int userId = this.GetUserId();
        AuthorizationStatus result = await githubService.GetAuthorizationStatusAsync(userId).ConfigureAwait(false);
        return Response<AuthorizationStatus>.Success("查询成功", result);
    }

    [HttpPost("Webhook")]
    public async Task<IActionResult> ProcessWebhookAsync([FromBody] GithubWebhookRequest request)
    {
        switch (HttpContext.Request.Headers["X-GitHub-Event"])
        {
            case "ping":
                return new JsonResult(new GithubWebhookResponse() { Message = "pong" });
            case "workflow_run" when request is { Action: "completed", WorkflowRun.Name: "Snap Hutao Alpha", WorkflowRun.PullRequests.Count: 0 }:
                await githubService.ProcessWorkflowRunEventAsync(request.WorkflowRun).ConfigureAwait(false);
                return new JsonResult(new GithubWebhookResponse() { Message = "Alpha sent to Discord" });
            case "release" when request is { Action: "released", Release: { } }:
                await githubService.ProcessReleaseEventAsync(request.Release).ConfigureAwait(false);
                return new JsonResult(new GithubWebhookResponse() { Message = "Release sent to Discord" });
            default:
                return new JsonResult(new GithubWebhookResponse() { Message = "Skip" });
        }
    }

    private RedirectResult RedirectToError(ReturnCode errorCode)
    {
        return Redirect($"https://passport.snapgenshin.cn/auth/error?code={errorCode:D}");
    }
}