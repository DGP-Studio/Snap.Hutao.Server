﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Extension;

/// <summary>
/// 信号量扩展
/// </summary>
public static class SemaphoreSlimExtension
{
    /// <summary>
    /// 异步进入信号量
    /// </summary>
    /// <param name="semaphoreSlim">信号量</param>
    /// <returns>可释放的对象，用于释放信号量</returns>
    public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphoreSlim)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        return new SemaphoreSlimReleaser(semaphoreSlim);
    }

    private struct SemaphoreSlimReleaser : IDisposable
    {
        private readonly SemaphoreSlim semaphoreSlim;

        public SemaphoreSlimReleaser(SemaphoreSlim semaphoreSlim)
        {
            this.semaphoreSlim = semaphoreSlim;
        }

        public void Dispose()
        {
            semaphoreSlim.Release();
        }
    }
}