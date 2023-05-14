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
    private readonly string diagnosticEmailAddress;

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
        diagnosticEmailAddress = smtp["DiagnosticEmailAddress"]!;

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
        return SendVerifyCodeMailAsync(emailAddress, GetRegistrationVerifyCodeMailBody(code));
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
        return SendVerifyCodeMailAsync(emailAddress, GetResetPasswordVerifyCodeMailBody(code));
    }

    /// <summary>
    /// 异步发送祈愿记录上传服务
    /// </summary>
    /// <param name="emailAddress">目标邮箱</param>
    /// <param name="expireAt">过期时间</param>
    /// <param name="tradeNumber">交易编号</param>
    /// <returns>任务</returns>
    public Task SendPurchaseGachaLogStorageServiceAsync(string emailAddress, string expireAt, string tradeNumber)
    {
        logger.LogInformation("Send GachaLog Mail to [{email}] with [{code}]", emailAddress, tradeNumber);
        return SendPurchaseServiceMailAsync(emailAddress, GetPurchaseGachaLogStorageServiceMailBody(expireAt, tradeNumber));
    }

    /// <summary>
    /// 异步发送开发者申请邮件
    /// </summary>
    /// <param name="emailAddress">申请账号</param>
    /// <returns>任务</returns>
    public Task SendOpenSourceLicenseNotificationApprovalAsync(string emailAddress)
    {
        return SendOpenSourceApplicationMailAsync(emailAddress, GetOpenSourceLicenseApplicationApprovalBody(emailAddress));
    }

    /// <summary>
    /// 异步发送深渊清理任务邮件
    /// </summary>
    /// <param name="task">任务名称</param>
    /// <param name="deletedRecordsCount">删除记录个数</param>
    /// <param name="deletedSpiralCount">删除深渊个数</param>
    /// <param name="removedKeys">移除的 Redis key 个数</param>
    /// <returns>任务</returns>
    public Task SendDiagnosticSpiralAbyssCleanJobAsync(string task, int deletedRecordsCount, int deletedSpiralCount, long removedKeys)
    {
        return SendDiagnosticMailAsync(diagnosticEmailAddress, GetDiagnosticSpiralAbyssCleanJobMailBody(task, deletedRecordsCount, deletedSpiralCount, removedKeys));
    }

    /// <summary>
    /// 异步发送开发者申请邮件
    /// </summary>
    /// <param name="userName">申请账号</param>
    /// <param name="url">维护网站</param>
    /// <param name="code">验证代码</param>
    /// <returns>任务</returns>
    public Task SendDiagnosticOpenSourceLicenseNotificationAsync(string userName, string url, string code)
    {
        return SendDiagnosticMailAsync(diagnosticEmailAddress, GetDiagnosticOpenSourceLicenseApplicationBody(userName, url, code));
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
                            max-width: 400px;
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
                            max-width: 400px;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetPurchaseGachaLogStorageServiceMailBody(string expireAt, string tradeNumber)
    {
        return $$"""
            <html>
                <head>
                    <style>
                        div {
                            background: linear-gradient(120deg,#f1c40f,#f39c12);
                            box-shadow: 4px 4px 8px #e67e2280;
                            max-width: 400px;
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
                        <h3>感谢您购买 Snap Hutao 祈愿记录上传服务</h3>
                        <p>服务有效期至</p>
                        <h2>{{expireAt}}</h2>
                        <p>请妥善保存此邮件，订单编号：{{tradeNumber}}</p>
                        <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                    </div>
            </body>
            </html>
            """;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetOpenSourceLicenseApplicationApprovalBody(string userName)
    {
        return $$"""
            <html>
            <head>
                <style>
                    div {
                        background: linear-gradient(120deg, #f1c40f, #f39c12);
                        box-shadow: 4px 4px 8px #e67e2280;
                        max-width: 400px;
                        padding: 16px;
                    }
                    hr {
                        background-color: #2c3e50;
                        height: 1px;
                        border: none;
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
                    <h3>胡桃开放平台开发者申请</h3>
                    <hr style="margin-top: 16px;"/>
                    <p>{{userName}}，你的开发者许可申请已经通过</p>
                    <hr style="margin-top: 16px;"/>
                    <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                </div>
            </body>
            </html>
            """;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDiagnosticSpiralAbyssCleanJobMailBody(string task, int deletedRecordsCount, int deletedSpiralCount, long removedKeys)
    {
        return $$"""
            <html>
            <head>
                <style>
                    div {
                        background: linear-gradient(120deg, #f1c40f, #f39c12);
                        box-shadow: 4px 4px 8px #e67e2280;
                        max-width: 400px;
                        padding: 16px;
                    }
                    hr {
                        background-color: #2c3e50;
                        height: 1px;
                        border: none;
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
                    <h3>Snap Hutao 服务端运行报告</h3>
                    <hr style="margin-top: 16px;"/>
                    <p>任务：<b>{{task}}</b></p>
                    <p>删除超时记录 <b>{{deletedRecordsCount}}</b> 条</p>
                    <p>删除深渊数据 <b>{{deletedSpiralCount}}</b> 条</p>
                    <p>删除 Redis 键 <b>{{removedKeys}}</b> 个</p>
                    <hr style="margin-top: 16px;"/>
                    <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                </div>
            </body>
            </html>
            """;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDiagnosticOpenSourceLicenseApplicationBody(string userName, string url, string code)
    {
        return $$"""
            <html>
            <head>
                <style>
                    div {
                        background: linear-gradient(120deg, #f1c40f, #f39c12);
                        box-shadow: 4px 4px 8px #e67e2280;
                        max-width: 400px;
                        padding: 16px;
                    }
                    hr {
                        background-color: #2c3e50;
                        height: 1px;
                        border: none;
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
                    <h3>胡桃开放平台开发者申请</h3>
                    <hr style="margin-top: 16px;"/>
                    <p>申请账号：<b>{{userName}}</b></p>
                    <p>维护网站：<a href="{{url}}">{{url}}</a></p>
                    <a href="https://homa.snapgenshin.com/Accession/ApproveOpenSourceLicense?userName={{userName}}&code={{code}}">批准</a>
                    <hr style="margin-top: 16px;"/>
                    <p style="margin: 0px;">DGP Studio 胡桃开发团队</p>
                </div>
            </body>
            </html>
            """;
    }

    private async Task SendVerifyCodeMailAsync(string emailAddress, string text)
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

    private async Task SendPurchaseServiceMailAsync(string emailAddress, string text)
    {
        MimeMessage mimeMessage = new()
        {
            Subject = "Snap Hutao 账号服务",
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

    private async Task SendOpenSourceApplicationMailAsync(string emailAddress, string text)
    {
        MimeMessage mimeMessage = new()
        {
            Subject = "胡桃开放平台",
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

    private async Task SendDiagnosticMailAsync(string emailAddress, string text)
    {
        MimeMessage mimeMessage = new()
        {
            Subject = "Snap Hutao 账号服务",
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