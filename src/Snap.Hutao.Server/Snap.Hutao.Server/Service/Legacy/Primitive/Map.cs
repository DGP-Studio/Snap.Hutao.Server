﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <inheritdoc cref="Dictionary{TKey, TValue}"/>
public class Map<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// 获取或新增
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="valueFactory">值工厂</param>
    /// <returns>值</returns>
    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (ContainsKey(key))
        {
            return this[key];
        }

        TValue value = valueFactory(key);

        Add(key, value);

        return value;
    }
}