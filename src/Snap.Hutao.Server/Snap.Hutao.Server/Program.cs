// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snap.Hutao.Server;

/// <summary>
/// 入口点
/// </summary>
public class Program
{
    /// <summary>
    /// 主方法
    /// </summary>
    /// <param name="args">参数</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        // Add services to the container.
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

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        WebApplication app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
