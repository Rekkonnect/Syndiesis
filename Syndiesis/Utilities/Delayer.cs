using System;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Utilities;

public sealed class Delayer
{
    private DateTime _nextUnblock = DateTime.MinValue;
    private Task? _delayTask;

    public bool IsWaiting => _delayTask is not null;

    public void SetFutureUnblock(TimeSpan span)
    {
        var next = DateTime.Now + span;
        ExtendNextUnblock(next);
    }

    public void ExtendNextUnblock(DateTime next)
    {
        if (next <= _nextUnblock)
        {
            return;
        }

        _nextUnblock = next;
    }

    public void CancelUnblock()
    {
        _nextUnblock = DateTime.MinValue;
        _delayTask = null;
    }

    public async Task WaitUnblock(CancellationToken cancellationToken)
    {
        // locking this is not crucial; it's probably not too bad spawning a
        // second task to track the time until the next unblock, compared to
        // the cost of entering the lock
        if (_delayTask is null)
        {
            _delayTask = MainWaitUnblock(cancellationToken);
        }

        await _delayTask;
        _delayTask = null;
    }

    private async Task MainWaitUnblock(CancellationToken cancellationToken)
    {
        while (true)
        {
            var remainder = _nextUnblock - DateTime.Now;
            if (remainder <= TimeSpan.Zero)
            {
                return;
            }

            await Task.Delay(remainder, cancellationToken);
            // we don't want to throw exceptions here
            if (cancellationToken.IsCancellationRequested)
                return;
        }
    }
}
