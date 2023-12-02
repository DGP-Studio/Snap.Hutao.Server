// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Design;

namespace Snap.Hutao.Server.Model.Context;

internal sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        DbContextOptionsBuilder<AppDbContext> builder = new();
        string connectionString = configuration.GetConnectionString("PrimaryMysql8")!;
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new AppDbContext(builder.Options);
    }
}