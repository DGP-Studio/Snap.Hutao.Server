// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Snap.Hutao.Server.Model.Context.Configuration;

/// <summary>
/// Json文本转换器
/// </summary>
/// <typeparam name="TProperty">实体类型</typeparam>
internal class JsonTextValueConverter<TProperty> : ValueConverter<TProperty, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTextValueConverter{TProperty}"/> class.
    /// </summary>
    public JsonTextValueConverter()
        : base(
            obj => JsonSerializer.Serialize(obj, default(JsonSerializerOptions)),
            str => JsonSerializer.Deserialize<TProperty>(str, default(JsonSerializerOptions))!)
    {
    }
}