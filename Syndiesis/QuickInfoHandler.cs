﻿using Avalonia.Input;
using Syndiesis.Controls.Editor.QuickInfo;

namespace Syndiesis;

public sealed class QuickInfoHandler(QuickInfoDisplayPopup popup)
{
    public readonly QuickInfoDisplayPopup Popup = popup;

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
                () => WaitAdjustOrHide(Popup));
        }
    }

    private bool EvaluateHide()
    {
        return !Popup.IsVisible;
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
        Popup.IsVisible = false;
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

        Popup.IsVisible = setVisible;
    }

    public sealed class PrepareShowingEventArgs(PointerEventArgs? lastPointerArgs)
    {
        public PointerEventArgs? LastPointerArgs { get; } = lastPointerArgs;

        public bool CancelShowing { get; set; }
    }
}
