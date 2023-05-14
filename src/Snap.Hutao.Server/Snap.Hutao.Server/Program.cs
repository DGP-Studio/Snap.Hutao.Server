// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Legacy;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snap.Hutao.Server;

/// <summary>
/// 主程序
/// </summary>
public class Program
{
    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="args">参数</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        services
            .AddHttpClient()
            .AddMemoryCache()
            .AddTransient<StatisticsService>()
            .AddTransient<RequestFilter>()
            .AddScoped<PassportService>()
            .AddSingleton<RankService>()
            .AddSingleton<MailService>()
            .AddSingleton<ExpireService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(builder.Configuration["Jwt"]!));

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

        services
            .AddAuthorization()
            .AddSingleton<IAuthorizationMiddlewareResultHandler, ResponseAuthorizationMiddlewareResultHandler>();

        services
            .AddResponseCompression()
            .AddControllers(options =>
            {
                // Disable non-nullable as [Required]
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options =>
            {
                JsonSerializerOptions jsonOptions = options.JsonSerializerOptions;

                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.WriteIndented = true;
            });

        services
            .AddQuartz(config =>
            {
                config
                    .UseMicrosoftDependencyInjectionJobFactory();
                config
                    .ScheduleJob<SpialAbyssRecordCleanJob>(t => t.StartNow().WithCronSchedule("0 0 4 1,16 * ?"))
                    .ScheduleJob<LegacyStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 5 */1 * * ?"));
            })
            .AddTransient<SpialAbyssRecordCleanJob>()
            .AddTransient<LegacyStatisticsRefreshJob>()
            .AddQuartzServer(options => options.WaitForJobsToComplete = true);

        services.AddCors(options => options.AddPolicy("CorsPolicy", options =>
        {
            options.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
        }));

        // services.AddDbContextPool<AppDbContext>(options => options.UseInMemoryDatabase("temp"));
        services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
        {
            string connectionString = builder.Configuration.GetConnectionString("LocalDb")!;
            serviceProvider.GetRequiredService<ILogger<Program>>().LogInformation("Using connection string: [{constr}]", connectionString);

            options
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("SpiralAbyss", new() { Version = "1.0", Title = "深渊统计", Description = "深渊统计数据" });
            options.SwaggerDoc("Passport", new() { Version = "1.0", Title = "胡桃账户", Description = "胡桃通行证" });
            options.SwaggerDoc("GachaLog", new() { Version = "1.0", Title = "祈愿记录", Description = "账户祈愿记录" });

            string xmlPath = Path.Combine(AppContext.BaseDirectory, "Snap.Hutao.Server.xml");
            options.IncludeXmlComments(xmlPath);
        });

        WebApplication app = builder.Build();
        MigrateDatabase(app);

        // 中间件的顺序敏感
        // https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0#middleware-order-1
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
    public static void MigrateDatabase(WebApplication app)
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
