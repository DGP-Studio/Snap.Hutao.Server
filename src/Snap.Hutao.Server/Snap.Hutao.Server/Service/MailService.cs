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

    public Task SendRegistrationVerifyCodeAsync(string emailAddress, string code, string language = "chs")
    {
        logger.LogInformation("Send Registration Mail to [{Email}] with [{Code}] in [{language}]", emailAddress, code, language);
        MailOptions options = new()
        {
            Subject = language == "chs" ? "Snap Hutao 账号安全" : "Snap Hutao Account Security",
            Address = emailAddress,
            Title = language == "chs" ? "感谢您注册 Snap Hutao 账号" : "Thank you for registering Snap Hutao account",
            RawContent = language == "chs" ? $"""
                <p>以下是您注册账号所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>如果您没有注册账号，请忽略此邮件，不会有任何事情发生。</p>
                """ : $"""
                <p>Here is the verification code you need for registering your account:</p>
                <span class="mail-code">{code}</span>
                <p>If you did not register an account, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }


    public Task SendResetPasswordVerifyCodeAsync(string emailAddress, string code, string language = "chs")
    {
        logger.LogInformation("Send ResetPassword Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = language == "chs" ? "Snap Hutao 账号安全" : "Snap Hutao Account Security",
            Address = emailAddress,
            Title = language == "chs" ? "您正在修改 Snap Hutao 账号密码" : "You are changing your Snap Hutao account password",
            RawContent = language == "chs" ? $"""
                <p>以下是您修改密码所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>如果您没有重置密码，请忽略此邮件，不会有任何事情发生。</p>
                """ : $"""
                <p>The following is the verification code you need to change your password:</p>
                <span class="mail-code">{code}</span>
                <p>If you did not reset your password, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }


    public Task SendCancelRegistrationVerifyCodeAsync(string emailAddress, string code, string language = "chs")
    {
        logger.LogInformation("Send CancelRegistration Mail to [{Email}] with [{Code}]", emailAddress, code);
        MailOptions options = new()
        {
            Subject = language == "chs" ? "Snap Hutao 账号安全" : "Snap Hutao Account Security",
            Address = emailAddress,
            Title = language == "chs" ? "您正在注销 Snap Hutao 账号" : "You are deleting your Snap Hutao account",
            RawContent = language == "chs" ? $"""
                <p><b>请注意：注销账号的操作是不可逆的</b></p>
                <p>以下是您注销账号所需的验证码：</p>
                <span class="mail-code">{code}</span>
                <p>如果您没有注销账号，请忽略此邮件，不会有任何事情发生。</p>
                """ : $"""
                <p><b>Please note: The operation of delete your account is not recoverable.</b></p>
                <p>The following is the verification code you need to delete your account:</p>
                <span class="mail-code">{code}</span>
                <p>If you did not delete your account, please ignore this email, nothing will happen.</p>
                """,
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }


    public Task SendPurchaseGachaLogStorageServiceAsync(string emailAddress, string expireAt, string tradeNumber, string language = "chs")
    {
        logger.LogInformation("Send GachaLog Mail to [{Email}] with [{Number}]", emailAddress, tradeNumber);
        MailOptions options = new()
        {
            Subject = language == "chs" ? "胡桃云服务" : "Hutao Cloud Service",
            Address = emailAddress,
            Title = language == "chs" ? "感谢您购买 Snap Hutao 祈愿记录上传服务" : "Thank you for purchasing Snap Hutao Wish Record Backup Service",
            RawContent = language == "chs" ? $"""
                <p>服务有效期至</p>
                <span class="mail-code">{expireAt}</span>
                <p>请妥善保存此邮件，订单编号：{tradeNumber}</p>
                """ : $"""
                <p>The service is valid until</p>
                <span class="mail-code">{expireAt}</span>
                <p>Please keep this email safe, order number: {tradeNumber}</p>
                """,
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }


    public Task SendOpenSourceLicenseNotificationApprovalAsync(string emailAddress, string language = "chs")
    {
        MailOptions options = new()
        {
            Subject = language == "chs" ? "胡桃开放平台" : "Hutao Open Platform",
            Address = emailAddress,
            Title = language == "chs" ? "胡桃开放平台开发者申请" : "Hutao Open Platform Developer Application",
            RawContent = language == "chs" ? $"{emailAddress}，你的开发者许可申请已经通过" : $"{emailAddress}, your developer license application has been approved",
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
        };

        return SendMailAsync(options);
    }


    public Task SendApproveOpenSourceLicenseNotificationAsync(string userName, string url, string code, string language = "chs")
    {
        MailOptions options = new()
        {
            Subject = language == "chs" ? "胡桃开放平台" : "Hutao Open Platform",
            Address = smtpOptions.DiagnosticEmailAddress,
            Title = language == "chs" ? "胡桃开放平台开发者申请" : "Hutao Open Platform Developer Application",
            RawContent = language == "chs" ? $"""
                <p>申请账号：{userName}</p>
                <p>维护网站：<a href="{url}">{url}</a></p>
                <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}">批准</a>
                """ : $"""
                <p>Application account: {userName}</p>
                <p>Maintenance website: <a href="{url}">{url}</a></p>
                <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={userName}&code={code}">Approve</a>
                """,
            Footer = language == "chs" ? "该邮件是 DGP-Studio 系统自动发送的，请勿回复" : "This email is automatically sent by the DGP-Studio system, please do not reply",
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
                        margin: auto;
                        display: block;
                        width: 25%;
                        text-align: center;
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
                    <span>&copy; 2023 <a href="https://github.com/DGP-Studio">DGP-Studio</a> | <a href="https://github.com/DGP-Studio/Snap.Hutao">Snap Hutao</a> Dev Team</span>
                </div>
            </div>
            </body>
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