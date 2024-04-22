using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using System;

namespace CSharpSyntaxEditor.Controls;

public abstract class BaseScrollBar : UserControl
{
    public event Action? ScrollChanged;

    protected SolidColorBrush AccentColorBrush = new(0xFF006066);

    public Color AccentColor
    {
        get => AccentColorBrush.Color;
        set
        {
            AccentColorBrush.Color = value;
        }
    }

    protected SolidColorBrush HoverColorBrush = new(0xFF009099);

    public Color HoverColor
    {
        get => HoverColorBrush.Color;
        set
        {
            HoverColorBrush.Color = value;
        }
    }

    protected SolidColorBrush BrushForHoverState(bool hovered)
    {
        return hovered ? HoverColorBrush : AccentColorBrush;
    }

    public double SmallStep { get; set; } = 1;

    public double LargeStep { get; set; } = 5;

    private double _minValue = 0;

    public double MinValue
    {
        get => _minValue;
        set
        {
            if (value > _maxValue)
            {
                MaxValue = value;
            }
            _minValue = value;
            UpdateIfNotPaused();
        }
    }

    private double _maxValue = 100;

    public double MaxValue
    {
        get => _maxValue;
        set
        {
            if (value < _minValue)
            {
                MinValue = value;
            }
            _maxValue = value;
            UpdateIfNotPaused();
        }
    }

    public double ValidValueRange => MaxValue - MinValue;

    private double _startPosition = 0;

    public double StartPosition
    {
        get => _startPosition;
        set
        {
            if (value > _endPosition)
            {
                throw new ArgumentException("start position cannot be set to after the end position");
            }
            value = Math.Clamp(value, _minValue, _maxValue);
            _startPosition = value;
            UpdateIfNotPaused();
        }
    }

    private double _endPosition = 5;

    public double EndPosition
    {
        get => _endPosition;
        set
        {
            if (value < _startPosition)
            {
                throw new ArgumentException("end position cannot be set to before the start position");
            }
            value = Math.Clamp(value, _minValue, _maxValue);
            _endPosition = value;
            UpdateIfNotPaused();
        }
    }

    public double ScrollWindowLength => EndPosition - StartPosition;

    public bool HasFullRangeWindow => ScrollWindowLength >= ValidValueRange;

    public bool CanScrollBelow => StartPosition > MinValue;
    public bool CanScrollAbove => EndPosition < MaxValue;

    private bool _hasAvailableScroll = true;

    public bool HasAvailableScroll
    {
        get => _hasAvailableScroll;
        set
        {
            _hasAvailableScroll = value;
            UpdateIfNotPaused();
        }
    }

    private bool _hasChanges = false;
    private int _updateLocks = 0;

    public void BeginUpdate()
    {
        _updateLocks++;
    }

    public void EndUpdate()
    {
        if (_updateLocks is 0)
            return;

        _updateLocks--;
        if (_hasChanges)
        {
            UpdateScroll();
        }
        _hasChanges = false;
    }

    private void UpdateIfNotPaused()
    {
        if (_updateLocks > 0)
        {
            _hasChanges = true;
        }
        else
        {
            UpdateScroll();
        }
    }

    protected override void OnMeasureInvalidated()
    {
        base.OnMeasureInvalidated();
        UpdateScroll();
    }

    private void UpdateScroll()
    {
        OnUpdateScroll();
        ScrollChanged?.Invoke();
    }

    protected abstract void OnUpdateScroll();

    public void Step(double step)
    {
        if (step is 0)
            return;

        if (step > 0)
        {
            double maxStep = MaxValue - EndPosition;
            if (maxStep is 0)
                return;

            if (step > maxStep)
            {
                step = maxStep;
            }

            BeginUpdate();
            EndPosition += step;
            StartPosition += step;
            EndUpdate();
        }
        else
        {
            double minStep = MinValue - StartPosition;
            if (minStep is 0)
                return;

            if (step < minStep)
            {
                step = minStep;
            }

            BeginUpdate();
            StartPosition += step;
            EndPosition += step;
            EndUpdate();
        }
    }
}

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
