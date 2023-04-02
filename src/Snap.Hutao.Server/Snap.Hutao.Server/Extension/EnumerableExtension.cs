// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Extension;

/// <summary>
/// 枚举器扩展
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// 将源转换为仅包含单个元素的枚举
    /// </summary>
    /// <typeparam name="TSource">源的类型</typeparam>
    /// <param name="source">源</param>
    /// <returns>集合</returns>
    public static IEnumerable<TSource> Enumerate<TSource>(this TSource source)
    {
        yield return source;
    }

    /// <summary>
    /// 增加计数
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="key">键</param>
    public static void Increase<TKey>(this Dictionary<TKey, int> dict, TKey key)
        where TKey : notnull
    {
        ++CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
    }

    /// <summary>
    /// 增加计数
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="key">键</param>
    /// <param name="value">增加的值</param>
    public static void Increase<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
        where TKey : notnull
    {
        // ref the value, so that we can manipulate it outside the dict.
        ref int current = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
        current += value;
    }

    /// <summary>
    /// 增加计数
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <param name="dict">字典</param>
    /// <param name="key">键</param>
    /// <returns>是否存在键值</returns>
    public static bool TryIncrease<TKey>(this Dictionary<TKey, int> dict, TKey key)
        where TKey : notnull
    {
        ref int value = ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
        if (!Unsafe.IsNullRef(ref value))
        {
            ++value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 转换到新类型的列表
    /// </summary>
    /// <typeparam name="TSource">原始类型</typeparam>
    /// <typeparam name="TResult">新类型</typeparam>
    /// <param name="list">列表</param>
    /// <param name="selector">选择器</param>
    /// <returns>新类型的列表</returns>
    public static List<TResult> SelectList<TSource, TResult>(this List<TSource> list, Func<TSource, TResult> selector)
    {
        Span<TSource> span = CollectionsMarshal.AsSpan(list);
        ref TSource reference = ref MemoryMarshal.GetReference(span);
        List<TResult> results = new(span.Length);
        for (int i = 0; i < span.Length; i++)
        {
            results.Add(selector(Unsafe.Add(ref reference, i)));
        }

        return results;
    }
}