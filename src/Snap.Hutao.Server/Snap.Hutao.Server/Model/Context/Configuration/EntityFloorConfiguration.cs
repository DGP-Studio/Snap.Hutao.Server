// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Model.Context.Configuration;

/// <summary>
/// 层信息配置
/// </summary>
public class EntityFloorConfiguration : IEntityTypeConfiguration<EntityFloor>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<EntityFloor> builder)
    {
        builder.Property(e => e.Levels)
            .HasColumnType("longtext")
            .HasConversion<JsonTextValueConverter<List<SimpleLevel>>>();
    }
}
