using Avalonia.Input;

namespace Syndiesis.Controls;

public class PointerDragHandler
{
    private Point? _sourcePoint;
    private Point _previousPoint;

    public event Action<Point>? DragStarted;
    public event Action<PointerDragArgs>? Dragged;
    public event Action? DragEnded;

    public bool IsActivelyDragging => _sourcePoint is not null;

    public void Attach(InputElement control)
    {
        control.PointerPressed += HandlePointerPressed;
        control.PointerMoved += HandlePointerMoved;
        control.PointerReleased += HandlePointerReleased;
    }

    public void InitiateDrag(PointerPressedEventArgs e)
    {
        var sourcePoint = e.GetPosition(null);
        InitiateDrag(sourcePoint);
    }

    public void StopDrag()
    {
        _sourcePoint = null;
        DragEnded?.Invoke();
    }

    private void InitiateDrag(Point sourcePoint)
    {
        _sourcePoint = sourcePoint;
        _previousPoint = sourcePoint;
        DragStarted?.Invoke(sourcePoint);
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(null);
        InitiateDrag(position);
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        StopDrag();
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_sourcePoint is null)
            return;

        var current = e.GetPosition(null);
        var delta = current - _previousPoint;
        if (delta == default)
            return;

        _previousPoint = current;
        var source = _sourcePoint!.Value;
        var totalDelta = current - source;
        var args = new PointerDragArgs
        {
            Delta = delta,
            TotalDelta = totalDelta,
            DragSourcePoint = source,
            CurrentPoint = current,
            SourcePointerEventArgs = e,
        };
        Dragged?.Invoke(args);
    }

    public class PointerDragArgs
    {
        public required Vector Delta { get; init; }
        public required Vector TotalDelta { get; init; }
        public required Point DragSourcePoint { get; init; }
        public required Point CurrentPoint { get; init; }
        public required PointerEventArgs SourcePointerEventArgs { get; init; }
    }
}
