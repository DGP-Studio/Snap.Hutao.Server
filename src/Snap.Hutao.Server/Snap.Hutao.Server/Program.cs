// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Quartz;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.GachaLog;
using Snap.Hutao.Server.Service.Legacy;
using Snap.Hutao.Server.Service.ReCaptcha;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Snap.Hutao.Server;

/// <summary>
/// 主程序
/// </summary>
public static class Program
{
    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="args">参数</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder appBuilder = WebApplication.CreateBuilder(args);

        IServiceCollection services = appBuilder.Services;

        // Services
        services
            .AddAuthorization()
            .AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()))
            .AddDbContextPool<AppDbContext>((serviceProvider, options) =>
            {
                string connectionString = appBuilder.Configuration.GetConnectionString("LocalDb")!;
                ILogger<AppDbContext> logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();
                logger.LogInformation("Using connection string: [{Constr}]", connectionString);

                options
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    /*.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)))*/;
            })
            .AddEndpointsApiExplorer()
            .AddHttpClient()
            .AddMemoryCache()
            .AddQuartz(config =>
            {
                config
                    .UseMicrosoftDependencyInjectionJobFactory();
                config
                    .ScheduleJob<GachaLogStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 30 */4 * * ?"))
                    .ScheduleJob<LegacyStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 5 */1 * * ?"))
                    .ScheduleJob<SpialAbyssRecordCleanJob>(t => t.StartNow().WithCronSchedule("0 0 4 1,16 * ?"));
            })
            .AddQuartzServer(options => options.WaitForJobsToComplete = true)
            .AddResponseCompression()
            .AddScoped<PassportService>()
            .AddSingleton<IAuthorizationMiddlewareResultHandler, ResponseAuthorizationMiddlewareResultHandler>()
            .AddSingleton<RankService>()
            .AddSingleton<MailService>()
            .AddSingleton<ExpireService>()
            .AddSingleton(appBuilder.Configuration.Get<AppOptions>()!)
            .AddSingleton(appBuilder.Configuration.GetSection("Smtp").Get<SmtpOptions>()!)
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("SpiralAbyss", new() { Version = "1.0", Title = "深渊统计", Description = "深渊统计数据" });
                options.SwaggerDoc("Passport", new() { Version = "1.0", Title = "胡桃账户", Description = "胡桃通行证" });
                options.SwaggerDoc("GachaLog", new() { Version = "1.0", Title = "祈愿记录", Description = "账户祈愿记录" });

                string xmlPath = Path.Combine(AppContext.BaseDirectory, "Snap.Hutao.Server.xml");
                options.IncludeXmlComments(xmlPath);
            })
            .AddTransient<GachaLogStatisticsRefreshJob>()
            .AddTransient<GachaLogStatisticsService>()
            .AddTransient<LegacyStatisticsRefreshJob>()
            .AddTransient<StatisticsService>()
            .AddTransient<ReCaptchaService>()
            .AddTransient<RequestFilter>()
            .AddTransient<SpialAbyssRecordCleanJob>();

        // Authentication
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(appBuilder.Configuration["Jwt"]!));

                options.TokenValidationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
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

        WebApplication app = appBuilder.Build();
        MigrateDatabase(app);

        // 中间件的顺序敏感
        // https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/#middleware-order
        // ExceptionHandler

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
        app.UseCors("CorsPolicy");

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
            option.SwaggerEndpoint("/swagger/Passport/swagger.json", "胡桃账户");
            option.SwaggerEndpoint("/swagger/GachaLog/swagger.json", "祈愿记录");

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
    private static void MigrateDatabase(IHost app)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (context.Database.IsRelational() && context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
    }
}