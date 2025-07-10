// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

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
        builder.UseMySql(connectionString, ServerVersion.Create(8, 0, 32, ServerType.MySql));
        return new AppDbContext(builder.Options);
    }
}