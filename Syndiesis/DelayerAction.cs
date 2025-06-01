using Garyon.Mechanisms;
using Garyon.Objects;

namespace Syndiesis;

public sealed class DelayerAction
{
    public readonly Delayer Delayer = new();
    public readonly CancellationTokenFactory CancellationTokenFactory = new();
    public Task? DelayerWaiter;

    public void ForceUnblock()
    {
        Delayer.CancelUnblock();
        CancellationTokenFactory.Cancel();
        DelayerWaiter = null;
    }

    public async Task WaitUnblockAsync()
    {
        await Delayer.WaitUnblock(CancellationTokenFactory.CurrentToken);
    }

    public void SetFutureUnblock(TimeSpan timeSpan, Func<Task> taskFactory)
    {
        Delayer.SetFutureUnblock(timeSpan);
        SetDelayerWaiter(taskFactory);
    }

    public void SetFutureUnblockFirst(TimeSpan timeSpan, Func<Task> taskFactory)
    {
        if (Delayer.IsWaiting)
            return;

        SetFutureUnblock(timeSpan, taskFactory);
    }

    private void SetDelayerWaiter(Func<Task> taskFactory)
    {
        if (IsTaskOverwritable(DelayerWaiter))
        {
            DelayerWaiter = taskFactory();
        }
    }

    private static bool IsTaskOverwritable(Task? task)
    {
        return task is null or { IsCompleted: true };
    }
}
