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
            Bodys =
            {
                ("h3", null, "感谢您注册 Snap Hutao 账号"),
                ("p", null, "以下是您注册账号所需的验证码："),
                ("h2", null, code),
                ("p", null, "如果您没有注册账号，请忽略此邮件，不会有任何事情发生。"),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
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
            Bodys =
            {
                ("h3", null, "您正在重置 Snap Hutao 账号密码"),
                ("p", null, "以下是您修改密码所需的验证码："),
                ("h2", null, code),
                ("p", null, "如果您没有重置密码，请忽略此邮件，不会有任何事情发生。"),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
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
            Bodys =
            {
                ("h3", null, "您正在注销 Snap Hutao 账号"),
                ("p", null, "以下是您注销账号所需的验证码："),
                ("h2", null, code),
                ("p", null, "如果您没有注销账号，请忽略此邮件，不会有任何事情发生。"),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
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
            Bodys =
            {
                ("h3", null, "感谢您购买 Snap Hutao 祈愿记录上传服务"),
                ("p", null, "服务有效期至"),
                ("h2", null, expireAt),
                ("p", null, $"请妥善保存此邮件，订单编号：{tradeNumber}"),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
        };

        return SendMailAsync(options);
    }

    public Task SendOpenSourceLicenseNotificationApprovalAsync(string emailAddress)
    {
        MailOptions options = new()
        {
            Subject = "胡桃开放平台",
            Address = emailAddress,
            Bodys =
            {
                ("h3", null, "胡桃开放平台开发者申请"),
                ("hr", null, string.Empty),
                ("p", null, $"{emailAddress}，你的开发者许可申请已经通过"),
                ("hr", null, string.Empty),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
        };

        return SendMailAsync(options);
    }

    public Task SendApproveOpenSourceLicenseNotificationAsync(string userName, string url, string code)
    {
        MailOptions options = new()
        {
            Subject = "胡桃开放平台",
            Address = smtpOptions.DiagnosticEmailAddress,
            Bodys =
            {
                ("h3", null, "胡桃开放平台开发者申请"),
                ("hr", null, string.Empty),
                ("p", null, $"申请账号：{userName}"),
                ("p", null, $"维护网站：<a href=\"{url}\">{url}</a>"),
                ("a", $"href=\"https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}\"", "批准"),
                ("hr", null, string.Empty),
                ("p", null, "DGP Studio 胡桃开发团队"),
            },
        };

        return SendMailAsync(options);
    }

    private static string ComposeMailBody(MailOptions options)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                    body {
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        height: 100vh;
                        margin: 0;
                    }
                    div {
                        background: linear-gradient(120deg, #f1c40f, #f39c12);
                        box-shadow: 4px 4px 8px #e67e2280;
                        max-width: 400px;
                        padding: 16px;
                    }
                    h3 {
                        margin: 0;
                        color: #2c3e50;
                    }
                    h2 {
                        background-color: #2c3e50;
                        padding: 12px;
                        color: #ecf0f1;
                        margin: 0;
                    }
                    p {
                        color: #34495e;
                        margin-bottom: 8px;
                    }
                    p:last-child {
                        margin-bottom: 0;
                    }
                </style>
            </head>
            <body>
                <div>
            {{BuildBodyHeader()}}
                </div>
            </body>
            </html>
            """;

        string BuildBodyHeader()
        {
            StringBuilder builder = new();
            foreach ((string tag, string? attribute, string content) in options.Bodys)
            {
                builder.Append("        ").AppendLine($"<{tag} {attribute}>{content}</{tag}>");
            }

            return builder.ToString();
        }
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