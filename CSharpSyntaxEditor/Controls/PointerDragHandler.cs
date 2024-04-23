using Avalonia;
using Avalonia.Input;
using System;

namespace CSharpSyntaxEditor.Controls;

public class PointerDragHandler
{
    private Point? _sourcePoint;
    private Point _previousPoint;

    public event Action? DragStarted;
    public event Action<PointerDragArgs>? Dragged;
    public event Action? DragEnded;

    public bool IsActivelyDragging => _sourcePoint is not null;

    public void Attach(InputElement control)
    {
        control.PointerPressed += HandlePointerPressed;
        control.PointerMoved += HandlePointerMoved;
        control.PointerReleased += HandlePointerReleased;
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(null);
        _sourcePoint = position;
        _previousPoint = position;
        DragStarted?.Invoke();
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _sourcePoint = null;
        DragEnded?.Invoke();
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_sourcePoint is null)
            return;

        var current = e.GetPosition(null);
        var delta = current - _previousPoint;
        _previousPoint = current;
        var source = _sourcePoint!.Value;
        var totalDelta = current - source;
        var args = new PointerDragArgs
        {
            Delta = delta,
            TotalDelta = totalDelta,
            DragSourcePoint = source,
            SourcePointerEventArgs = e,
        };
        Dragged?.Invoke(args);
    }

    public class PointerDragArgs
    {
        public required Vector Delta { get; init; }
        public required Vector TotalDelta { get; init; }
        public required Point DragSourcePoint { get; init; }
        public required PointerEventArgs SourcePointerEventArgs { get; init; }
    }
}
