// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <inheritdoc cref="Dictionary{TKey, TValue}"/>
public class Map<TKey, TValue> : Dictionary<TKey, TValue>
    where TKey : notnull
    where TValue : new()
{
    /// <summary>
    /// 初始化一个新的映射
    /// </summary>
    public Map()
        : base()
    {
    }

    public Map(int size)
        : base(size)
    {
    }

    /// <summary>
    /// 初始化一个新的映射
    /// </summary>
    /// <param name="dict">复制的字典</param>
    public Map(IDictionary<TKey, TValue> dict)
        : base(dict)
    {
    }

    /// <summary>
    /// 获取或新增
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="valueFactory">值工厂</param>
    /// <returns>值</returns>
    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (!TryGetValue(key, out TValue? value))
        {
            value = valueFactory(key);
            Add(key, value);
        }

        return value;
    }

    /// <summary>
    /// 获取或新增
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>值</returns>
    internal TValue GetOrNew(TKey key)
    {
        if (!TryGetValue(key, out TValue? value))
        {
            value = new();
            Add(key, value);
        }

        return value;
    }
}
