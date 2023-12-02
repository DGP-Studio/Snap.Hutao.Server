// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Metadata;

namespace Snap.Hutao.Server.Model.Context;

public sealed class MetadataDbContext : DbContext
{
    public MetadataDbContext(DbContextOptions<MetadataDbContext> options)
        : base(options)
    {
    }

    public DbSet<GachaEventInfo> GachaEvents { get; set; }

    public DbSet<KnownItem> KnownItems { get; set; }
}