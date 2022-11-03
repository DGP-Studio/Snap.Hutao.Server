// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quartz;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Legacy;
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
            .AddMemoryCache()
            .AddTransient<StatisticsService>()
            .AddTransient<RequestFilter>()
            .AddSingleton<RankService>();

        services
            .AddControllers()
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

        builder.Configuration.AddEnvironmentVariables();
        services.AddDbContextPool<AppDbContext>(optionsBuilder =>
        {
            string connectionString = builder.Configuration.GetConnectionString("LocalDb");

            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Version = "1.0.0.0", Title = "数据", Description = "数据状态" });

            // c.SwaggerDoc("v2", new() { Version = "1.0.0.0", Title = "统计数据", Description = "获取详细的深渊纵深数据" });
            // c.SwaggerDoc("v3", new() { Version = "1.0.0.0", Title = "记录操作", Description = "提交记录，查询提交状态" });
            // c.SwaggerDoc("v4", new() { Version = "1.0.0.0", Title = "记录操作", Description = "提交记录，查询提交状态" });
            // c.SwaggerDoc("v5", new() { Version = "1.0.0.0", Title = "记录操作", Description = "提交记录，查询提交状态" });
            string xmlFile = $"Snap.Hutao.Server.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        WebApplication app = builder.Build();
        MigrateDatabase(app);

        app.UseSwagger();
        app.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint("/swagger/v1/swagger.json", "数据 API");

            // option.SwaggerEndpoint("/swagger/v2/swagger.json", "数据详情 API");
            // option.SwaggerEndpoint("/swagger/v4/swagger.json", "数据详情2 API");
            // option.SwaggerEndpoint("/swagger/v3/swagger.json", "物品信息 API");
            // option.SwaggerEndpoint("/swagger/v5/swagger.json", "角色展柜 API");
        });

        app.UseHttpsRedirection();
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
