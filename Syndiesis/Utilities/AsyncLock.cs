using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Utilities;

// Inspired by a ChatGPT suggestion, with extra
public sealed class AsyncLock
{
    private volatile bool _isLocked = false;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool IsLocked => _isLocked;

    public LockReleaser Lock()
    {
        _semaphore.Wait();
        return PerformLock();
    }

    public async Task<LockReleaser> LockAsync()
    {
        await _semaphore.WaitAsync();
        return PerformLock();
    }

    public async Task<LockReleaser> LockAsync(CancellationToken token)
    {
        await _semaphore.WaitAsync(token);
        return PerformLock();
    }

    private LockReleaser PerformLock()
    {
        Debug.Assert(!_isLocked, "expected the lock to be unset");
        _isLocked = true;
        return new(this);
    }

    public readonly record struct LockReleaser(AsyncLock AsyncLock)
        : IDisposable
    {
        public void Dispose()
        {
            AsyncLock._semaphore.Release();
            AsyncLock._isLocked = false;
        }
    }
}
