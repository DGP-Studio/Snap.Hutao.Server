// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using MailKit.Net.Smtp;
using MimeKit;
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

    public Task SendRegistrationVerifyCodeAsync(string emailAddress, string code, string language = "CHS")
    {
        logger.LogInformation("Send Registration Mail to [{Email}] with [{Code}] in [{language}]", emailAddress, code, language);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "Snap Hutao 通行证安全"
                : "Snap Hutao Passport Security",
            Address = emailAddress,
            Title = language == "CHS"
                ? "感谢您注册 Snap Hutao 通行证"
                : "Thank you for registering Snap Hutao Passport",
            RawContent = language == "CHS"
                ? $"""
                <p>以下是您注册通行证所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>该验证码将于<span class="mail-hint">15</span>分钟后过期。</p>
                <p>如果您没有注册通行证，请忽略此邮件，不会有任何事情发生。</p>
                """
                : $"""
                <p>The following is the verification code you need for the registration of your passport:</p>
                <span class="mail-code">{code}</span>
                <p>This code will expire in <span class="mail-hint">15</span> minutes.</p>
                <p>If you are not trying to register an passport, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendResetPasswordVerifyCodeAsync(string emailAddress, string code, string language = "CHS")
    {
        logger.LogInformation("Send ResetPassword Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "Snap Hutao 通行证安全"
                : "Snap Hutao Passport Security",
            Address = emailAddress,
            Title = language == "CHS"
                ? "您正在修改 Snap Hutao 通行证密码"
                : "You are changing your Snap Hutao passport password",
            RawContent = language == "CHS"
                ? $"""
                <p>以下是您修改密码所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>该验证码将于<span class="mail-hint">15</span>分钟后过期。</p>
                <p>如果您没有重置密码，请忽略此邮件，不会有任何事情发生。</p>
                """
                : $"""
                <p>The following is the verification code you need to change your password:</p>
                <span class="mail-code">{code}</span>
                <p>This code will expire in <span class="mail-hint">15</span> minutes.</p>
                <p>If you are not trying to reset your password, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendResetUsernameVerifyCodeAsync(string emailAddress, string code, string language = "CHS")
    {
        logger.LogInformation("Send ResetPassword Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "Snap Hutao 通行证安全"
                : "Snap Hutao Passport Security",
            Address = emailAddress,
            Title = language == "CHS"
                ? "您正在修改 Snap Hutao 通行证邮箱"
                : "You are changing your Snap Hutao passport email",
            RawContent = language == "CHS"
                ? $"""
                <p>以下是您修改邮箱所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>该验证码将于<span class="mail-hint">15</span>分钟后过期。</p>
                <p>如果您没有修改邮箱，请忽略此邮件，不会有任何事情发生。</p>
                """
                : $"""
                <p>The following is the verification code you need to change your email:</p>
                <span class="mail-code">{code}</span>
                <p>This code will expire in <span class="mail-hint">15</span> minutes.</p>
                <p>If you are not trying to change your email, please ignore this emai, nothing will happen.</p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendCancelRegistrationVerifyCodeAsync(string emailAddress, string code, string language = "CHS")
    {
        logger.LogInformation("Send CancelRegistration Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "Snap Hutao 通行证安全"
                : "Snap Hutao Passport Security",
            Address = emailAddress,
            Title = language == "CHS"
                ? "您正在注销 Snap Hutao 通行证"
                : "You are deleting your Snap Hutao Passport",
            RawContent = language == "CHS"
                ? $"""
                <p><b style="font-size: 20px;">请注意：注销通行证的操作是不可逆的</b></p>
                <p>以下是您注销通行证所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>该验证码将于<span class="mail-hint">15</span>分钟后过期。</p>
                <p>如果您没有注销通行证，请忽略此邮件，不会有任何事情发生。</p>
                """
                : $"""
                <p><b style="font-size: 20px;">Please note: The operation of deleting your passport is not recoverable.</b></p>
                <p>The following is the verification code you need to delete your passport:</p>
                <span class="mail-code">{code}</span>
                <p>This code will expire in <span class="mail-hint">15</span> minutes.</p>
                <p>If you are not trying to delete your passport, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendPurchaseGachaLogStorageServiceAsync(string emailAddress, string expireAt, string tradeNumber, string language = "CHS")
    {
        logger.LogInformation("Send GachaLog Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "胡桃云服务"
                : "Snap Hutao Cloud Service",
            Address = emailAddress,
            Title = language == "CHS"
                ? "感谢您赞助 Snap Hutao 祈愿记录上传服务"
                : "Thank you for sponsor Snap Hutao Wish Record Backup Service",
            RawContent = language == "CHS"
                ? $"""
                <p>服务有效期至</p>
                <span class="mail-date">{expireAt}</span>
                <p>请妥善保存此邮件，订单编号：<span class="mail-hint">{tradeNumber}</span></p>
                """
                : $"""
                <p>The service is valid until</p>
                <span class="mail-date">{expireAt}</span>
                <p>Please keep this email safe, order number: <span class="mail-hint">{tradeNumber}</span></p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendPurchaseGachaLogStorageServiceNoSuchUserAsync(string emailAddress, string redeemCode, string tradeNumber, string language = "CHS")
    {
        logger.LogInformation("Send GachaLog Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "胡桃云服务"
                : "Snap Hutao Cloud Service",
            Address = emailAddress,
            Title = language == "CHS"
                ? "感谢您赞助 Snap Hutao 祈愿记录上传服务"
                : "Thank you for sponsor Snap Hutao Wish Record Backup Service",
            RawContent = language == "CHS"
                ? $"""
                <p>由于您尚未注册胡桃通行证，现向您发放胡桃云兑换码，有效期为 3 天</p>
                <p>您可以在 Snap Hutao 内胡桃通行证页面注册后进行兑换</p>
                <span class="mail-date">{redeemCode}</span>
                <p>请妥善保存此邮件，订单编号：<span class="mail-hint">{tradeNumber}</span></p>
                """
                : $"""
                <p>Since you have not registered for Snap Hutao Passport, we are issuing you a redeem code, valid for 3 days</p>
                <p>You can redeem it after registering on the Snap Hutao Passport page in Snap Hutao</p>
                <span class="mail-date">{redeemCode}</span>
                <p>Please keep this email safe, order number: <span class="mail-hint">{tradeNumber}</span></p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendPurchaseCdnServiceAsync(string emailAddress, string expireAt, string tradeNumber, string language = "CHS")
    {
        logger.LogInformation("Send CDN Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "胡桃云服务"
                : "Snap Hutao Cloud Service",
            Address = emailAddress,
            Title = language == "CHS"
                ? "感谢您赞助 Snap Hutao 胡桃云 CDN 更新加速服务"
                : "Thank you for sponsor Snap Hutao Cloud CDN Update Acceleration Service",
            RawContent = language == "CHS"
                ? $"""
                <p>服务有效期至</p>
                <span class="mail-date">{expireAt}</span>
                <p>请妥善保存此邮件，订单编号：<span class="mail-hint">{tradeNumber}</span></p>
                """
                : $"""
                <p>The service is valid until</p>
                <span class="mail-date">{expireAt}</span>
                <p>Please keep this email safe, order number: <span class="mail-hint">{tradeNumber}</span></p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendPurchaseCdnServiceNoSuchUserAsync(string emailAddress, string redeemCode, string tradeNumber, string language = "CHS")
    {
        logger.LogInformation("Send CDN Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "胡桃云服务"
                : "Snap Hutao Cloud Service",
            Address = emailAddress,
            Title = language == "CHS"
                ? "感谢您赞助 Snap Hutao 胡桃云 CDN 更新加速服务"
                : "Thank you for sponsor Snap Hutao Cloud CDN Update Acceleration Service",
            RawContent = language == "CHS"
                ? $"""
                <p>由于您尚未注册胡桃通行证，现向您发放胡桃云兑换码，有效期为 3 天</p>
                <p>您可以在 Snap Hutao 内胡桃通行证页面注册后进行兑换</p>
                <span class="mail-date">{redeemCode}</span>
                <p>请妥善保存此邮件，订单编号：<span class="mail-hint">{tradeNumber}</span></p>
                """
                : $"""
                <p>Since you have not registered for Snap Hutao Passport, we are issuing you a redeem code, valid for 3 days</p>
                <p>You can redeem it after registering on the Snap Hutao Passport page in Snap Hutao</p>
                <span class="mail-date">{redeemCode}</span>
                <p>Please keep this email safe, order number: <span class="mail-hint">{tradeNumber}</span></p>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendOpenSourceLicenseNotificationApprovalAsync(string emailAddress, string language = "CHS")
    {
        MailOptions options = new()
        {
            Subject = language == "CHS"
                ? "胡桃开放平台"
                : "Snap Hutao Open Platform",
            Address = emailAddress,
            Title = language == "CHS"
                ? "胡桃开放平台开发者申请"
                : "Snap Hutao Open Platform Developer Application",
            RawContent = language == "CHS"
                ? $"""<span class="mail-hint">{emailAddress}</span>，你的开发者许可申请已经通过"""
                : $"""<span class="mail-hint">{emailAddress}</span>, your developer license application has been approved""",
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }

    public Task SendApproveOpenSourceLicenseNotificationAsync(string userName, string url, string code, string language = "CHS")
    {
        MailOptions options = new()
        {
            Subject =
                language == "CHS"
                ? "胡桃开放平台"
                : "Hutao Open Platform",
            Address = smtpOptions.DiagnosticEmailAddress,
            Title = language == "CHS"
                ? "胡桃开放平台开发者申请"
                : "Snap Hutao Open Platform Developer Application",
            RawContent = language == "CHS"
                ? $"""
                <p>申请通行证：<span class="mail-hint">{userName}</span></p>
                <p>维护网站：<a href="{url}">{url}</a></p>
                <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}">批准</a>
                """
                : $"""
                <p>Application passport: <span class="mail-hint">{userName}</span></p>
                <p>Maintenance website: <a href="{url}">{url}</a></p>
                <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}">Approve</a>
                """,
            Footer = language == "CHS"
                ? "该邮件由 DGP Studio 系统自动生成，请勿回复"
                : "This email is automatically sent by the DGP Studio system, please do not reply",
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
                <title>DGP Studio Mail Template</title>
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
                        line-height: 1.5;
                    }

                    #mail-content {
                        width: 100%;
                        display: flex;
                        white-space: pre-wrap;
                        flex-direction: column;
                        align-items: flex-start;
                        justify-content: center;
                        text-align: left;
                    }

                    .mail-code {
                        font-family: 'Monaco', monospace;
                        border: 1px solid #DAE1E9;
                        letter-spacing: 4px;
                        padding: 20px 8px;
                        border-radius: 4px;
                        background-color: #F4F7FA;
                        color: #3a9aed;
                        margin: auto;
                        display: block;
                        width: 60%;
                        text-align: center;
                        font-weight: bold;
                        font-size: 30px;
                    }

                    .mail-date {
                        position: relative;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        font-family: 'Monaco', monospace;
                        border: 1px solid #DAE1E9;
                        padding: 20px 8px;
                        border-radius: 4px;
                        background-color: #F4F7FA;
                        color: #febd2b;
                        width: 100%;
                        box-sizing: border-box;
                        text-align: center;
                        font-weight: bold;
                        font-size: 20px;
                    }

                    .mail-hint {
                        font-weight: bold;
                        color: #fd6243;
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
            </head>
            <body>
            <div id="mail-table">
                <div id="mail-body">
                    <img alt="DGP Studio"
                         id="dgp-logo"
                         src="https://img.alicdn.com/imgextra/i3/1797064093/O1CN01sjsnty1g6e14c9BwZ_!!1797064093.png">
                    <div id="mail-title">{{options.Title}}</div>
                    <div id="mail-content">{{options.RawContent}}</div>
                </div>
                <div id="mail-footer">
                    <span>{{options.Footer}}</span>
                    <span>&copy; 2022-2025 <a href="https://github.com/DGP-Studio">DGP Studio</a> | <a href="https://github.com/DGP-Studio/Snap.Hutao">Snap Hutao</a> Dev Team. All rights reserved.</span>
                </div>
            </div>
            </body>
            </html>
            """;
    }

    private Task SendMailAsync(MailOptions options)
    {
        return SendMimeMailAsync(options);
    }

    private async Task SendHttpMailAsync(MailOptions options)
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

    private async Task SendMimeMailAsync(MailOptions options)
    {
        MimeMessage mimeMessage = new()
        {
            Subject = options.Subject,
            From =
            {
                new MailboxAddress("DGP Studio", smtpOptions.UserName),
            },
            To =
            {
                new MailboxAddress(options.Address, options.Address),
            },
            Body = new TextPart("html")
            {
                Text = ComposeMailBody(options),
            },
        };

        using (SmtpClient client = new())
        {
            await client.ConnectAsync(smtpOptions.Server).ConfigureAwait(false);
            await client.AuthenticateAsync(smtpOptions.UserName, smtpOptions.Password).ConfigureAwait(false);
            await client.SendAsync(mimeMessage).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
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