using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Diagnostics;

namespace Syndiesis.Controls;

public sealed class EventArgsHistoryContainer
{
    public static readonly EventArgsHistoryContainer Instance = new();

    public PointerEventArgs? Pointer { get; private set; }

    public void Register(Control control)
    {
        control.PointerMoved += CapturePointerEventArgs;
        control.PointerEntered += CapturePointerEventArgs;
        control.PointerExited += CapturePointerEventArgs;
        control.PointerPressed += CapturePointerEventArgs;
        control.PointerReleased += CapturePointerEventArgs;
        control.PointerWheelChanged += CapturePointerEventArgs;
    }

    private void CapturePointerEventArgs(object? sender, PointerEventArgs e)
    {
        Pointer = e;
        Debug.WriteLine($"{DateTime.Now.Ticks} - PointerEventArgs registered");
    }
}
