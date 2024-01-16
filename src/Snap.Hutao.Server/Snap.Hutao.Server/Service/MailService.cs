// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Option;

namespace Snap.Hutao.Server.Service;

public sealed class MailService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<MailService> logger;
    private readonly SmtpOptions smtpOptions;
    private readonly string mailerSecret;

    public MailService(IHttpClientFactory httpClientFactory, AppOptions appOptions, ILogger<MailService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        smtpOptions = appOptions.Smtp;
        mailerSecret = appOptions.MailerSecret;
    }

    public Task SendRegistrationVerifyCodeAsync(string emailAddress, string code)
    {
        logger.LogInformation("Send Registration Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = "Snap Hutao 账号安全",
            Address = emailAddress,
            Title = "感谢您注册 Snap Hutao 账号",
            RawContent = $"""
                <p>以下是您注册账号所需的验证码：</p>
                <td style="padding-bottom: 10px; padding-top: 10px;">
                    <span class="mail-code">{code}</span>
                </td>
                <p>如果您没有注册账号，请忽略此邮件，不会有任何事情发生。</p>
                """,
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    public Task SendResetPasswordVerifyCodeAsync(string emailAddress, string code)
    {
        logger.LogInformation("Send ResetPassword Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = "Snap Hutao 账号安全",
            Address = emailAddress,
            Title = "您正在修改 Snap Hutao 账号密码",
            RawContent = $"""
                <p>以下是您修改密码所需的验证码：</p>
                <td style="padding-bottom: 10px; padding-top: 10px;">
                    <span class="mail-code">{code}</span>
                </td>
                <p>如果您没有重置密码，请忽略此邮件，不会有任何事情发生。</p>
                """,
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    public Task SendCancelRegistrationVerifyCodeAsync(string emailAddress, string code)
    {
        logger.LogInformation("Send CancelRegistration Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = "Snap Hutao 账号安全",
            Address = emailAddress,
            Title = "您正在注销 Snap Hutao 账号",
            RawContent = $"""
                <p>以下是您注销账号所需的验证码：</p>
                <td style="padding-bottom: 10px; padding-top: 10px;">
                    <span class="mail-code">{code}</span>
                </td>
                <p>如果您没有注销账号，请忽略此邮件，不会有任何事情发生。</p>
                """,
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    public Task SendPurchaseGachaLogStorageServiceAsync(string emailAddress, string expireAt, string tradeNumber)
    {
        logger.LogInformation("Send GachaLog Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = "胡桃云服务",
            Address = emailAddress,
            Title = "感谢您购买 Snap Hutao 祈愿记录上传服务",
            RawContent = $"""
                <p>服务有效期至</p>
                <td style="padding-bottom: 10px; padding-top: 10px;">
                    <span class="mail-code">{expireAt}</span>
                </td>
                <p>请妥善保存此邮件，订单编号：{tradeNumber}</p>
                """,
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    public Task SendOpenSourceLicenseNotificationApprovalAsync(string emailAddress)
    {
        MailOptions options = new()
        {
            Subject = "胡桃开放平台",
            Address = emailAddress,
            Title = "胡桃开放平台开发者申请",
            RawContent = $"{emailAddress}，你的开发者许可申请已经通过",
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    public Task SendApproveOpenSourceLicenseNotificationAsync(string userName, string url, string code)
    {
        MailOptions options = new()
        {
            Subject = "胡桃开放平台",
            Address = smtpOptions.DiagnosticEmailAddress,
            Title = "胡桃开放平台开发者申请",
            RawContent = $"""
                <p>申请账号：{userName}</p>
                维护网站：<a href="{url}">{url}</a>
                <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}">批准</a>
                """,
            Footer = "DGP Studio | 胡桃开发团队",
        };

        return SendMailAsync(options);
    }

    private static string ComposeMailBody(MailOptions options)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>DGP-Studio Mail Template</title>
            </head>
            <body>
            <div id="mail-table">
                <div id="mail-body">
                    <img alt="google"
                         id="mail-logo"
                         src="https://img.alicdn.com/imgextra/i2/1797064093/O1CN01UWZbb81g6e146Uazl_!!1797064093.png">
                    <div id="mail-title">{{options.Title}}</div>
                    <div id="mail-content">{{options.RawContent}}</div>
                </div>
                <div id="mail-footer">
                    <span>{{options.Footer}}</span>
                    <span>&copy; 2023 DGP-Studio</span>
                </div>
            </div>
            </body>
            <style>
                #mail-table {
                    margin: auto;
                    display: flex;
                    flex-direction: column;
                    justify-content: center;
                    align-items: center;
                    row-gap: 10px;
                    padding: 20px;
                }

                #mail-body {
                    border: 1px solid #dadce0;
                    border-radius: 5px;
                    padding: 40px 20px;
                    line-height: 20px;
                    text-align: center;
                    display: flex;
                    row-gap: 16px;
                    flex-direction: column;
                    justify-content: flex-start;
                    align-items: center;
                }

                #mail-logo {
                    width: auto;
                    height: 80px;
                    object-fit: contain;
                }

                #mail-title {
                    font-family: 'Google Sans', Roboto, RobotoDraft, Helvetica, Arial, sans-serif;
                    font-size: 24px;
                    color: rgba(0, 0, 0);
                    padding-bottom: 32px;
                    text-align: center;
                    border-bottom: thin solid #dadce0;
                }

                #mail-content {
                    width: 100%;
                    text-align: left;
                    white-space: pre-wrap;
                }

                .mail-code {
                    font-family:'Monaco', monospace;
                    border:1px solid #DAE1E9;
                    letter-spacing:2px;
                    padding:5px 8px;
                    border-radius:4px;
                    background-color:#F4F7FA;
                    color:#2E7BC4;
                }

                #mail-footer {
                    font-family: Roboto-Regular, Helvetica, Arial, sans-serif;
                    width: 100%;
                    display: flex;
                    flex-direction: column;
                    text-align: center;
                    font-size: 11px;
                    color: rgb(0 0 0/0.54);
                    line-height: 18px;
                }
            </style>
            </html>
            """;
    }

    private async Task SendMailAsync(MailOptions options)
    {
        using (HttpClient httpClient = httpClientFactory.CreateClient())
        {
            MailData data = new()
            {
                From = "DGP Studio <no-reply@snapgenshin.cn>",
                To = options.Address,
                Subject = options.Subject,
                BodyHtml = ComposeMailBody(options),
            };

            httpClient.DefaultRequestHeaders.Authorization = new("SECRET", mailerSecret);
            await httpClient.PostAsJsonAsync("https://mailer.snapgenshin.cn/api/sendEmail", data).ConfigureAwait(false);
        }
    }

    private sealed class MailData
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = default!;

        [JsonPropertyName("to")]
        public string To { get; set; } = default!;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = default!;

        [JsonPropertyName("bodyHtml")]
        public string BodyHtml { get; set; } = default!;
    }
}