// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Extension;

public static class SemaphoreSlimExtension
{
    public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphoreSlim)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        return new SemaphoreSlimReleaser(semaphoreSlim);
    }

    private readonly struct SemaphoreSlimReleaser : IDisposable
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