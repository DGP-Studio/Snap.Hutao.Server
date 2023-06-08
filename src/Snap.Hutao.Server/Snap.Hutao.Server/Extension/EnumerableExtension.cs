// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Extension;

/// <summary>
/// 枚举器扩展
/// </summary>
public static class EnumerableExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IncreaseOne<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        where TKey : notnull
        where TValue : struct, IIncrementOperators<TValue>
    {
        ++CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
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
        List<TResult> results = new(list.Count);
        foreach (ref TSource source in CollectionsMarshal.AsSpan(list))
        {
            results.Add(selector(source));
        }

        return results;
    }
}