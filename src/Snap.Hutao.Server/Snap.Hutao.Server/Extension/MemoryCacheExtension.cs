// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Extension;

/// <summary>
/// 内存缓存拓展
/// </summary>
public static class MemoryCacheExtension
{
    /// <summary>
    /// 在内存缓存中创建标志
    /// </summary>
    /// <param name="memoryCache">内存缓存</param>
    /// <param name="name">标志名称</param>
    /// <returns>消除标志</returns>
    /// <exception cref="InvalidOperationException">已经存在名为name的标志</exception>
    public static IDisposable Flag(this IMemoryCache memoryCache, string name)
    {
        if (memoryCache.TryGetValue(name, out object? value))
        {
            throw new InvalidOperationException($"已经存在名为 '{name}' 的标志");
        }

        memoryCache.Set(name, true);

        return new MemoryCacheFlagRemover(memoryCache, name);
    }

    private readonly struct MemoryCacheFlagRemover : IDisposable
    {
        private readonly IMemoryCache memoryCache;
        private readonly string name;

        public MemoryCacheFlagRemover(IMemoryCache memoryCache, string name)
        {
            this.memoryCache = memoryCache;
            this.name = name;
        }

        public void Dispose()
        {
            memoryCache.Remove(name);
        }
    }
}