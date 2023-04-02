// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
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
                    ClockSkew = TimeSpan.FromSeconds(10),
                };
            });

        services
            .AddIdentityCore<HutaoUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddAuthorization();

        services
            .AddResponseCompression()
            .AddControllers(options =>
            {
                // Disable non-nullable as [Required]
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(o =>
            {
                JsonSerializerOptions options = o.JsonSerializerOptions;

                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                options.PropertyNameCaseInsensitive = true;
                options.WriteIndented = true;
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
            .AddQuartzServer(config => config.WaitForJobsToComplete = true);

        services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin().AllowCredentials();
        }));

        services.AddDbContextPool<AppDbContext>(optionsBuilder =>
        {
            string connectionString = builder.Configuration.GetConnectionString("LocalDb")!;
            Console.WriteLine($"Using connection string:\n [{connectionString}]");
            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Version = "1.0.0.0", Title = "Hutao API", Description = "Genshin Impact Open API" });
            string xmlFile = $"Snap.Hutao.Server.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        WebApplication app = builder.Build();
        MigrateDatabase(app);

        app.UseResponseCompression();

        app.UseSwagger();
        app.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint("/swagger/v1/swagger.json", "Hutao API v2");
        });

        app.UseHttpsRedirection();

        // 顺序敏感
        app.UseAuthentication();
        app.UseAuthorization();

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
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }
    }
}
