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
                <meta name="format-detection" content="email=no"/>
                <meta name="format-detection" content="date=no"/>
                <style nonce="64080I9IgI8JfUgIIJ2Qfg">
                    .awl a {color: #FFFFFF; text-decoration: none;}
                    .abml a {color: #000000; font-family: Roboto-Medium,Helvetica,Arial,sans-serif; font-weight: bold; text-decoration: none;}
                    .adgl a {color: rgba(0, 0, 0, 0.87); text-decoration: none;}
                    .afal a {color: #b0b0b0; text-decoration: none;}
                    @media screen and (min-width: 600px) {
                        .v2sp {padding: 6px 30px 0px;}
                        .v2rsp {padding: 0px 10px;}
                    }
                    @media screen and (min-width: 600px) {
                        .mdv2rw {
                            padding: 40px 40px;
                            box-shadow: 0 0 10px 0 rgba(255, 165, 0, 0.5); /* Add orange shadow */
                        }
                    }
                </style>
            </head>
            <body style="margin: 0; padding: 0;" bgcolor="#FFFFFF">
                <table width="100%" height="100%" style="min-width: 348px;" border="0" cellspacing="0" cellpadding="0" lang="en">
                    <tr height="32" style="height: 32px;"><td></td></tr>
                    <tr align="center">
                        <td>
                            <div itemscope itemtype="//schema.org/EmailMessage">
                                <div itemprop="action" itemscope itemtype="//schema.org/ViewAction">
                                    <meta itemprop="name" content="Review Activity"/>
                                </div>
                            </div>
                            <table border="0" cellspacing="0" cellpadding="0" style="padding-bottom: 20px; max-width: 516px; min-width: 220px;">
                                <tr>
                                    <td width="8" style="width: 8px;"></td>
                                    <td>
                                        <div style="border-style: solid; border-width: thin; border-color:#dadce0; border-radius: 8px; padding: 40px 20px;" align="center" class="mdv2rw">
                                            <img src="https://img.alicdn.com/imgextra/i2/1797064093/O1CN01UWZbb81g6e146Uazl_!!1797064093.png" width="134" height="78" aria-hidden="true" style="margin-bottom: 16px;" alt="Google">
                                            <div style="font-family: 'Google Sans',Roboto,RobotoDraft,Helvetica,Arial,sans-serif;border-bottom: thin solid #dadce0; color: rgba(0,0,0,0.87); line-height: 32px; padding-bottom: 24px;text-align: center; word-break: break-word;">
                                                <div style="font-size: 24px;">{{BuildBodyTitle()}}</div>
                                            </div>
                                            <div style="font-family: Roboto-Regular,Helvetica,Arial,sans-serif; font-size: 14px; color: rgba(0,0,0,0.87); line-height: 20px;padding-top: 20px; text-align: left;">
                                                {{BuildBodyHeader()}}
                                            </div>
                                        </div>
                                        <div style="text-align: left;">
                                            <div style="font-family: Roboto-Regular,Helvetica,Arial,sans-serif;color: rgba(0,0,0,0.54); font-size: 11px; line-height: 18px; padding-top: 12px; text-align: center;">
                                                <div>{{BuildBodyFooter()}}</div>
                                                <div style="direction: ltr;">&copy; 2023 DGP-Studio</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td width="8" style="width: 8px;"></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr height="32" style="height: 32px;"><td></td></tr>
                </table>
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