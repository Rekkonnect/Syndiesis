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

    private readonly Delayer _delayer = new();
    private Task? _delayerWaiter;
    private readonly CancellationTokenFactory _delayerCancellationTokenFactory = new();

    private PointerEventArgs? _lastPointerMoved;

    public event Action<PrepareShowingEventArgs>? PrepareShowing;

    public void RegisterMovementHandling(Control control)
    {
        control.PointerMoved += HandlePointerMoved;
    }

    public void ForceDisplay()
    {
        _delayer.CancelUnblock();
        _delayerCancellationTokenFactory.Cancel();
        _delayerWaiter = null;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        _lastPointerMoved = e;
        var hidden = EvaluateHide();
        if (hidden)
        {
            _delayer.SetFutureUnblock(AppSettings.Instance.HoverInfoDelay);

            if (_delayerWaiter is null or { IsCompleted: true })
            {
                _delayerWaiter = WaitShow(sender as Control);
            }
        }
    }

    private bool EvaluateHide()
    {
        if (_popup.IsPointerOver)
            return false;

        Hide();
        return true;
    }

    private async Task WaitShow(Control? sender)
    {
        await _delayer.WaitUnblock(_delayerCancellationTokenFactory.CurrentToken);

        if (sender?.IsPointerOver is true)
        {
            Show();
        }
    }

    private void Hide()
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
}
