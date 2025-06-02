// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Microsoft.AspNetCore.Hosting;
using Quartz;
using Quartz.AspNetCore;
using Quartz.Simpl;
using Sentry;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Discord;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Passport;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Option;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Afdian;
using Snap.Hutao.Server.Service.Announcement;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Discord;
using Snap.Hutao.Server.Service.Distribution;
using Snap.Hutao.Server.Service.Expire;
using Snap.Hutao.Server.Service.GachaLog;
using Snap.Hutao.Server.Service.GachaLog.Statistics;
using Snap.Hutao.Server.Service.Github;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.Legacy.PizzaHelper;
using Snap.Hutao.Server.Service.Licensing;
using Snap.Hutao.Server.Service.Ranking;
using Snap.Hutao.Server.Service.ReCaptcha;
using Snap.Hutao.Server.Service.Redeem;
using Snap.Hutao.Server.Service.RoleCombat;
using Snap.Hutao.Server.Service.Sentry;
using Snap.Hutao.Server.Service.Telemetry;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Snap.Hutao.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder appBuilder = WebApplication.CreateBuilder(args);

        appBuilder.WebHost.UseSentry(options =>
        {
            options.Release = $"{DateTimeOffset.Now:yyyy.M.d.Hm}";
            options.Dsn = "http://7de19654a539bfdd56a798ce89e85137@host.docker.internal:9510/7";
            options.TracesSampleRate = 1D;
            options.SendDefaultPii = true;
        });

        appBuilder.Services.AddSentryTunneling();

        IServiceCollection services = appBuilder.Services;

        AppOptions appOptions = appBuilder.Configuration.GetSection("App").Get<AppOptions>()!;

        // Services
        services
            .AddAuthorization()
            .AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()))
            .AddDbContextPool<AppDbContext>((serviceProvider, options) =>
            {
                string connectionString = appBuilder.Configuration.GetConnectionString("PrimaryMysql8")!;
                serviceProvider
                    .GetRequiredService<ILogger<AppDbContext>>()
                    .LogInformation("AppDbContext Using connection string: [{Constr}]", connectionString);

                options
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
            })
            .AddDbContextPool<MetadataDbContext>((serviceProvider, options) =>
            {
                string connectionString = appBuilder.Configuration.GetConnectionString("MetadataMysql8")!;
                serviceProvider
                    .GetRequiredService<ILogger<MetadataDbContext>>()
                    .LogInformation("MetadataDbContext Using connection string: [{Constr}]", connectionString);

                options
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
            })
            .AddEndpointsApiExplorer()
            .AddHttpClient()
            .AddMemoryCache()
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["retcode"] = context.ProblemDetails.Status ?? (int)ReturnCode.InternalStateException;
                    context.ProblemDetails.Extensions["message"] = context.Exception?.Message ?? string.Empty;
                };
            })
            .AddQuartz(config =>
            {
                config.UseJobFactory<MicrosoftDependencyInjectionJobFactory>();
                config.ScheduleJob<GachaLogStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 30 */1 * * ?"));
                config.ScheduleJob<LegacyStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 5 */1 * * ?"));
                config.ScheduleJob<SpiralAbyssRecordCleanJob>(t => t.StartNow().WithCronSchedule("0 0 4 16 * ?"));
                config.ScheduleJob<RoleCombatStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 10 */1 * * ?"));
                config.ScheduleJob<RoleCombatRecordCleanJob>(t => t.StartNow().WithCronSchedule("0 0 4 1 * ?"));
            })
            .AddQuartzServer(options => options.WaitForJobsToComplete = true)
            .AddResponseCompression()
            .AddScoped<AccessionService>()
            .AddScoped<AnnouncementService>()
            .AddScoped<DistributionService>()
            .AddScoped<GachaLogService>()
            .AddScoped<GithubService>()
            .AddScoped<PassportService>()
            .AddScoped<PassportVerificationService>()
            .AddScoped<PizzaHelperRecordService>()
            .AddScoped<RecordService>()
            .AddScoped<SpiralAbyssStatisticsService>()
            .AddScoped<TelemetryService>()
            .AddScoped<RedeemService>()
            .AddSingleton<AfdianWebhookService>()
            .AddSingleton<CdnExpireService>()
            .AddSingleton<DiscordService>()
            .AddSingleton<GachaLogExpireService>()
            .AddSingleton<IAuthorizationMiddlewareResultHandler, ResponseAuthorizationMiddlewareResultHandler>()
            .AddSingleton<IRankService, RankService>()
            .AddSingleton<MailService>()
            .AddSingleton<ISentryUserFactory, SentryUserFactory>()
            .AddSingleton(appOptions)
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("SpiralAbyss", new() { Version = "1.0", Title = "深渊统计", Description = "深渊统计数据" });
                options.SwaggerDoc("RoleCombat", new() { Version = "1.0", Title = "剧演统计", Description = "幻想真境剧诗" });
                options.SwaggerDoc("Passport", new() { Version = "1.0", Title = "胡桃账户", Description = "胡桃通行证" });
                options.SwaggerDoc("GachaLog", new() { Version = "1.0", Title = "祈愿记录", Description = "账户祈愿记录管理" });
                options.SwaggerDoc("Distribution", new() { Version = "1.0", Title = "分发管理", Description = "胡桃分发管理" });
                options.SwaggerDoc("Services", new() { Version = "1.0", Title = "服务管理", Description = "维护专用管理接口，调用需要维护权限" });

                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Snap.Hutao.Server.xml"));
            })
            .AddTransient<GachaLogStatisticsRefreshJob>()
            .AddTransient<GachaLogStatisticsService>()
            .AddTransient<GithubApiService>()
            .AddTransient<LegacyStatisticsRefreshJob>()
            .AddTransient<StatisticsService>()
            .AddTransient<ReCaptchaService>()
            .AddTransient<RoleCombatRecordCleanJob>()
            .AddTransient<RoleCombatService>()
            .AddTransient<RoleCombatStatisticsRefreshJob>()
            .AddTransient<SpiralAbyssRecordCleanJob>()
            .AddTransient<ValidateCdnPermission>()
            .AddTransient<ValidateGachaLogPermission>()
            .AddTransient<ValidateMaintainPermission>();

        // Authentication
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = appOptions.GetJwtSecurityKey(),
                    ValidateIssuer = true,
                    ValidIssuer = "homa.snapgenshin.com",
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10),
                };
            });

        // Identity
        services
            .AddIdentityCore<HutaoUser>(options =>
            {
                PasswordOptions passwordOptions = options.Password;

                passwordOptions.RequiredLength = 8;
                passwordOptions.RequiredUniqueChars = 0;
                passwordOptions.RequireLowercase = false;
                passwordOptions.RequireUppercase = false;
                passwordOptions.RequireNonAlphanumeric = false;
                passwordOptions.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        // Mvc
        services
            .AddControllers(options =>
            {
                // Disable non-nullable as [Required]
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options =>
            {
                JsonSerializerOptions jsonOptions = options.JsonSerializerOptions;

                jsonOptions.PropertyNamingPolicy = null;
                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.WriteIndented = true;
            });

        // Discord Bot
        appBuilder.Host.ConfigureDiscordBot<HutaoServerBot>((hostContext, botContext) =>
        {
            botContext.OwnerIds = appOptions.Discord.OwnerIds.Select(id => (Snowflake)id);
            botContext.Intents = GatewayIntents.LibraryRecommended | GatewayIntents.DirectMessages;
            botContext.Token = appOptions.Discord.Token;
        });

        WebApplication app = appBuilder.Build();
        MigrateDatabase(app);

        // 中间件的顺序敏感
        // https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/#middleware-order
        // ExceptionHandler
        app.UseExceptionHandler();

        // HSTS
        // HttpsRedirection
        app.UseHttpsRedirection();

        // Static Files
        app.UseDefaultFiles(new DefaultFilesOptions()
        {
            DefaultFileNames =
            {
                "index.html",
            },
        });
        app.UseStaticFiles();

        // Routes
        // CORS
        app.UseCors();

        // Authentication
        app.UseAuthentication();

        // Authorization
        app.UseAuthorization();

        // Custom
        app.UseResponseCompression();

        app.UseSwagger();
        app.UseSwaggerUI(option =>
        {
            option.RoutePrefix = "doc";
            option.DocumentTitle = "Hutao API Documentation";
            option.InjectStylesheet("/css/style.css");

            option.SwaggerEndpoint("/swagger/SpiralAbyss/swagger.json", "深渊统计");
            option.SwaggerEndpoint("/swagger/RoleCombat/swagger.json", "剧演统计");
            option.SwaggerEndpoint("/swagger/Passport/swagger.json", "胡桃账户");
            option.SwaggerEndpoint("/swagger/GachaLog/swagger.json", "祈愿记录");
            option.SwaggerEndpoint("/swagger/Distribution/swagger.json", "分发管理");

            option.DefaultModelExpandDepth(-1);
            option.DocExpansion(DocExpansion.None);
        });

        // Endpoint
        app.MapControllers();
        app.Run();
    }

    /// <summary>
    /// 迁移数据库
    /// </summary>
    /// <param name="app">app</param>
    [Conditional("RELEASE")]
    private static void MigrateDatabase(IHost app)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (context.Database.IsRelational() && context.Database.GetPendingMigrations().Any())
            {
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
                context.Database.Migrate();
            }
        }
    }
}