// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Extension;

public static class MemoryCacheExtension
{
    [Obsolete]
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