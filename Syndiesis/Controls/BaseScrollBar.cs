using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace Syndiesis.Controls;

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
                EndPosition = value;
            }
            value = Math.Clamp(value, _minValue, _maxValue);
            _startPosition = value;
            UpdateIfNotPaused();
        }
    }

    public double DisplayStartPosition => DisplayPosition(_startPosition);

    private double _endPosition = 5;

    public double EndPosition
    {
        get => _endPosition;
        set
        {
            if (value < _startPosition)
            {
                StartPosition = value;
            }
            value = Math.Clamp(value, _minValue, _maxValue);
            _endPosition = value;
            UpdateIfNotPaused();
        }
    }

    public double DisplayEndPosition => DisplayPosition(_endPosition);

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

    private double _minVisibleStep = 0;

    public double MinVisibleStep
    {
        get => _minVisibleStep;
        set
        {
            _minVisibleStep = value;
            UpdateIfNotPaused();
        }
    }

    public abstract ScrollBarStepButtonContainer PreviousButtonContainer { get; }
    public abstract ScrollBarStepButtonContainer NextButtonContainer { get; }

    public abstract Shape PreviousIconShape { get; }
    public abstract Shape NextIconShape { get; }
    public abstract Rectangle DraggableRectangle { get; }

    private bool _hasChanges = false;
    private int _updateLocks = 0;

    private double _dragInitialStartPosition;

    protected readonly PointerDragHandler DragHandler = new();

    public void SetAvailableScrollOnScrollableWindow()
    {
        const double ratioThreshold = 0.985;
        var windowRatio = ScrollWindowLength / ValidValueRange;
        HasAvailableScroll = windowRatio < ratioThreshold;
    }

    public UpdateBlock BeginUpdateBlock()
    {
        return new(this);
    }

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

    protected void InitializeBrushes()
    {
        PreviousButtonContainer.HoverBackgroundBrush = AccentColorBrush;
        NextButtonContainer.HoverBackgroundBrush = AccentColorBrush;

        PreviousButtonContainer.ContentFillBrush = AccentColorBrush;
        NextButtonContainer.ContentFillBrush = AccentColorBrush;

        PreviousButtonContainer.HoverContentFillBrush = HoverColorBrush;
        NextButtonContainer.HoverContentFillBrush = HoverColorBrush;

        PreviousButtonContainer.Background = Background;
        NextButtonContainer.Background = Background;

        PreviousIconShape.Fill = AccentColorBrush;
        NextIconShape.Fill = AccentColorBrush;
        DraggableRectangle.Fill = AccentColorBrush;
    }

    protected void InitializeEvents()
    {
        PreviousButtonContainer.button.Click += HandlePreviousClick;
        NextButtonContainer.button.Click += HandleNextClick;

        var draggableRectangle = DraggableRectangle;
        draggableRectangle.PointerEntered += HandlePointerOverRectangle;
        draggableRectangle.PointerExited += HandlePointerOverRectangle;
        draggableRectangle.PointerMoved += HandlePointerOverRectangle;
    }

    protected void InitializeDraggableHandler()
    {
        DragHandler.DragStarted += HandleDragStart;
        DragHandler.Dragged += HandleDragging;
        DragHandler.Attach(DraggableRectangle);
    }

    protected abstract void HandleDragging(PointerDragHandler.PointerDragArgs args);

    private void HandleDragStart(Point startPoint)
    {
        _dragInitialStartPosition = StartPosition;
    }

    protected double CalculateStep(double totalStep, double dimensionLength)
    {
        var previousOffset = StartPosition - _dragInitialStartPosition;
        var previousDimensionOffsetLength = previousOffset / ValidValueRange * dimensionLength;
        var valueStep = totalStep - previousDimensionOffsetLength;
        return valueStep / dimensionLength * ValidValueRange;
    }

    private void HandlePointerOverRectangle(object? sender, PointerEventArgs e)
    {
        var draggableRectangle = DraggableRectangle;
        bool isHovered = draggableRectangle.IsPointerOver || DragHandler.IsActivelyDragging;
        var brush = BrushForHoverState(isHovered);
        draggableRectangle.Fill = brush;
    }

    public double DisplayPosition(double actual)
    {
        var minStep = MinVisibleStep;
        var position = actual;
        if (minStep is 0)
            return position;

        var stepCount = position / minStep;
        var roundedSteps = Math.Round(stepCount);
        return roundedSteps * minStep;
    }

    private void HandlePreviousClick(object? sender, RoutedEventArgs e)
    {
        Step(-SmallStep);
    }

    private void HandleNextClick(object? sender, RoutedEventArgs e)
    {
        Step(SmallStep);
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
        BasicUpdateScroll();
        if (!HasAvailableScroll)
        {
            return;
        }
        OnUpdateScroll();
        ScrollChanged?.Invoke();
    }

    private void BasicUpdateScroll()
    {
        SetAvailableScrollVisibility(HasAvailableScroll);
    }

    private void SetAvailableScrollVisibility(bool value)
    {
        DraggableRectangle.IsVisible = value;
        PreviousButtonContainer.Enabled = value;
        NextButtonContainer.Enabled = value;
    }

    protected abstract void OnUpdateScroll();

    public void SetStartPositionPreserveLength(double start)
    {
        var current = StartPosition;
        var offset = start - current;
        Step(offset);
    }

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

    public readonly struct UpdateBlock : IDisposable
    {
        private readonly BaseScrollBar _scrollBar;

        public UpdateBlock(BaseScrollBar scrollBar)
        {
            _scrollBar = scrollBar;
            _scrollBar.BeginUpdate();
        }

        public void Dispose()
        {
            _scrollBar.EndUpdate();
        }
    }
}
