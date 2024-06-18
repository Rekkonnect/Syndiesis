using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace Syndiesis.Controls;

public partial class HorizontalScrollBar : BaseScrollBar
{
    public override ScrollBarStepButtonContainer PreviousButtonContainer => leftButton;
    public override ScrollBarStepButtonContainer NextButtonContainer => rightButton;
    public override Rectangle DraggableRectangle => draggableRectangle;
    public override Rectangle DraggableRegion => draggableContainerRectangle;

    public override Shape PreviousIconShape => leftIcon;
    public override Shape NextIconShape => rightIcon;

    public HorizontalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeEvents();
        InitializeDraggableHandler();
    }

    protected override void HandleDraggableRegionPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(draggableRectangle);
        var dimension = position.X;
        var dimensionLength = draggableRectangleCanvas.Bounds.Width;
        var left = Canvas.GetLeft(draggableRectangle);
        var width = draggableRectangle.Width;
        var centerOffset = width / 2;
        var offset = left;
        var end = width;
        HandleDraggable(e, dimension, dimensionLength, centerOffset, end);
    }

    protected override void HandleDragging(PointerDragHandler.PointerDragArgs args)
    {
        var step = CalculateStep(args.TotalDelta.X, draggableRectangleCanvas.Bounds.Width);
        Step(step);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = -e.Delta.X * SmallStep;
        Step(delta);
    }

    protected override void OnUpdateScroll()
    {
        var availableWidth = draggableRectangleCanvas.Bounds.Width;
        var valueRange = ValidValueRange;
        var window = ScrollWindowLength;
        var start = DisplayStartPosition - MinValue;

        Canvas.SetLeft(draggableRectangle, PixelValue(start));
        draggableRectangle.Width = PixelValue(window);

        double PixelValue(double scrollValue)
        {
            if (valueRange is 0)
                return 0;
            return scrollValue / valueRange * availableWidth;
        }
    }
}
