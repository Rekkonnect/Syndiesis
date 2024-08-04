using Avalonia.Controls;
using Avalonia.Input;
using Syndiesis.Controls.Editor;
using Syndiesis.Utilities;
using System;
using System.Threading.Tasks;

namespace Syndiesis;

public sealed class QuickInfoHandler(QuickInfoDisplayPopup popup)
{
    private readonly QuickInfoDisplayPopup _popup = popup;

    private PointerEventArgs? _lastPointerMoved;

    private readonly DelayerAction _appearanceDelayerAction = new();
    private readonly DelayerAction _disappearanceDelayerAction = new();

    public event Action<PrepareShowingEventArgs>? PrepareShowing;

    public void RegisterMovementHandling(Control control)
    {
        control.PointerMoved += HandlePointerMoved;
    }

    public void ForceDisplay()
    {
        _appearanceDelayerAction.ForceUnblock();
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        _lastPointerMoved = e;
        var hidden = EvaluateHide();
        if (hidden)
        {
            _appearanceDelayerAction.SetFutureUnblock(
                AppSettings.Instance.HoverInfoDelay,
                () => WaitShow(sender as Control));
        }
        else
        {
            _disappearanceDelayerAction.SetFutureUnblockFirst(
                TimeSpan.FromMilliseconds(200),
                () => WaitAdjustOrHide(_popup));
        }
    }

    private bool EvaluateHide()
    {
        return !_popup.IsVisible;
    }

    private async Task WaitShow(Control? sender)
    {
        await _appearanceDelayerAction.WaitUnblockAsync();

        if (sender?.IsPointerOver is true)
        {
            Show();
        }
    }

    private async Task WaitAdjustOrHide(Control? sender)
    {
        await _disappearanceDelayerAction.WaitUnblockAsync();

        if (sender?.IsPointerOver is false)
        {
            Hide();

            // Attempt to show the popup at a new location
            Show();
        }
    }

    public void Hide()
    {
        _popup.IsVisible = false;
    }

    private void Show()
    {
        var setVisible = true;

        if (PrepareShowing is not null)
        {
            var args = new PrepareShowingEventArgs(_lastPointerMoved);
            PrepareShowing.Invoke(args);
            setVisible = !args.CancelShowing;
        }

        _popup.IsVisible = setVisible;
    }

    public sealed class PrepareShowingEventArgs(PointerEventArgs? lastPointerArgs)
    {
        public PointerEventArgs? LastPointerArgs { get; } = lastPointerArgs;

        public bool CancelShowing { get; set; }
    }

    private class DelayerAction
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
}
