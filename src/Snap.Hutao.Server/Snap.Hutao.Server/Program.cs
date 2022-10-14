// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quartz;
using Snap.Hutao.Server.Job;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Service.Legacy;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snap.Hutao.Server;

/// <summary>
/// ��ڵ�
/// </summary>
public class Program
{
    /// <summary>
    /// ������
    /// </summary>
    /// <param name="args">����</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        services
            .AddMemoryCache()
            .AddTransient<StatisticsService>();

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
                    .ScheduleJob<SpialAbyssRecordClearJob>(t => t.StartNow().WithCronSchedule("0 0 4 1,16 * ?"))
                    .ScheduleJob<LegacyStatisticsRefreshJob>(t => t.StartNow().WithCronSchedule("0 5 */1 * * ?"));
            })
            .AddTransient<SpialAbyssRecordClearJob>()
            .AddTransient<LegacyStatisticsRefreshJob>()
            .AddQuartzServer(config => config.WaitForJobsToComplete = true);

        services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin().AllowCredentials();
        }));

        builder.Configuration.AddEnvironmentVariables();
        services.AddDbContextPool<AppDbContext>(optionsBuilder =>
        {
            string connectionString = builder.Configuration.GetConnectionString("Snap_DB");

            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuted, LogLevel.Debug)));
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Version = "1.0.0.0", Title = "��¼����", Description = "�ύ��¼����ѯ�ύ״̬" });
            c.SwaggerDoc("v2", new() { Version = "1.0.0.0", Title = "ͳ������", Description = "��ȡ��ϸ����Ԩ��������" });

            // c.SwaggerDoc("v3", new() { Version = "1.0.0.0", Title = "��¼����", Description = "�ύ��¼����ѯ�ύ״̬" });
            // c.SwaggerDoc("v4", new() { Version = "1.0.0.0", Title = "��¼����", Description = "�ύ��¼����ѯ�ύ״̬" });
            // c.SwaggerDoc("v5", new() { Version = "1.0.0.0", Title = "��¼����", Description = "�ύ��¼����ѯ�ύ״̬" });
            string xmlFile = $"Snap.Hutao.Server.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        WebApplication app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint("/swagger/v1/swagger.json", "��¼���� API");
            option.SwaggerEndpoint("/swagger/v2/swagger.json", "�������� API");

            // option.SwaggerEndpoint("/swagger/v4/swagger.json", "��������2 API");
            // option.SwaggerEndpoint("/swagger/v3/swagger.json", "��Ʒ��Ϣ API");
            // option.SwaggerEndpoint("/swagger/v5/swagger.json", "��ɫչ�� API");
        });

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
