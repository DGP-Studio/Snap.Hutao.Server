// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using MailKit.Net.Smtp;
using MimeKit;
using System.Runtime.CompilerServices;

namespace Snap.Hutao.Server.Service;

/// <summary>
/// 邮件服务
/// </summary>
public sealed class MailService
{
    private readonly ILogger<MailService> logger;
    private readonly string server;
    private readonly string userName;
    private readonly string password;

    /// <summary>
    /// 构造一个新的邮件服务
    /// </summary>
    /// <param name="configuration">配置</param>
    /// <param name="logger">日志器</param>
    public MailService(IConfiguration configuration, ILogger<MailService> logger)
    {
        this.logger = logger;

        IConfigurationSection smtp = configuration.GetSection("Smtp");
        server = smtp["Server"]!;
        userName = smtp["UserName"]!;
        password = smtp["Password"]!;

        logger.LogInformation("Initialized with UserName:{userName} Password:{password}", userName, password);
    }

    /// <summary>
    /// 异步发送注册验证码
    /// </summary>
    /// <param name="emailAddress">目标邮箱</param>
    /// <param name="code">验证码</param>
    /// <returns>任务</returns>
    public Task SendRegistrationVerifyCodeAsync(string emailAddress, string code)
    {
        logger.LogInformation("Send Registration Mail to [{email}] with [{code}]", emailAddress, code);
        return SendMailAsync(emailAddress, GetRegistrationVerifyCodeMailBody(code));
    }

    /// <summary>
    /// 异步发送重置密码验证码
    /// </summary>
    /// <param name="emailAddress">目标邮箱</param>
    /// <param name="code">验证码</param>
    /// <returns>任务</returns>
    public Task SendResetPasswordVerifyCodeAsync(string emailAddress, string code)
    {
        logger.LogInformation("Send ResetPassword Mail to [{email}] with [{code}]", emailAddress, code);
        return SendMailAsync(emailAddress, GetResetPasswordVerifyCodeMailBody(code));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetRegistrationVerifyCodeMailBody(string code)
    {
        return $$"""
            <html>
                <head>
                    <style>
                        div {
                            background: linear-gradient(120deg,#f1c40f,#f39c12);
                            box-shadow: 4px 4px 8px #e67e2280;
                            max-width: 480px;
                            padding: 16px;
                        }
                        h3 {
                            margin: 0px;
                            color: #2c3e50;
                        }
                        h2 {
                            background-color: #2c3e50;
                            padding: 12px;
                            color: #ecf0f1;
                        }
                        p {
                            color: #34495e;
                        }
                    </style>
                </head>
                <body>
                    <div>
                        <h3>感谢您注册 Snap Hutao 账号</h3>
                        <p>以下是您注册账号所需的验证码：</p>
                        <h2>{{code}}</h2>
                        <p>如果您没有注册账号，请忽略此邮件，不会有任何事情发生。</p>
                        <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                    </div>
                </body>
            </html>
            """;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetResetPasswordVerifyCodeMailBody(string code)
    {
        return $$"""
            <html>
                <head>
                    <style>
                        div {
                            background: linear-gradient(120deg,#f1c40f,#f39c12);
                            box-shadow: 4px 4px 8px #e67e2280;
                            max-width: 480px;
                            padding: 16px;
                        }
                        h3 {
                            margin: 0px;
                            color: #2c3e50;
                        }
                        h2 {
                            background-color: #2c3e50;
                            padding: 12px;
                            color: #ecf0f1;
                        }
                        p {
                            color: #34495e;
                        }
                    </style>
                </head>
                <body>
                    <div>
                        <h3>您正在重置 Snap Hutao 账号密码</h3>
                        <p>以下是您修改密码所需的验证码：</p>
                        <h2>{{code}}</h2>
                        <p>如果您没有重置密码，请忽略此邮件，不会有任何事情发生。</p>
                        <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                    </div>
                </body>
            </html>
            """;
    }

    private async Task SendMailAsync(string emailAddress, string text)
    {
        MimeMessage mimeMessage = new()
        {
            Subject = "Snap Hutao 账号安全",
            From =
            {
                new MailboxAddress("DGP Studio", userName),
            },
            To =
            {
                new MailboxAddress(emailAddress, emailAddress),
            },
            Body = new TextPart("html")
            {
                Text = text,
            },
        };

        using (SmtpClient client = new())
        {
            await client.ConnectAsync(server).ConfigureAwait(false);
            await client.AuthenticateAsync(userName, password).ConfigureAwait(false);
            await client.SendAsync(mimeMessage).ConfigureAwait(false);
            await client.DisconnectAsync(true).ConfigureAwait(false);
        }
    }
}