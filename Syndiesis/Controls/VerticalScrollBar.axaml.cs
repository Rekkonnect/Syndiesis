using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace Syndiesis.Controls;

public partial class VerticalScrollBar : BaseScrollBar
{
    public override ScrollBarStepButtonContainer PreviousButtonContainer => upButton;
    public override ScrollBarStepButtonContainer NextButtonContainer => downButton;
    public override Rectangle DraggableRectangle => draggableRectangle;
    public override Rectangle DraggableRegion => draggableContainerRectangle;

    public override Shape PreviousIconShape => upIcon;
    public override Shape NextIconShape => downIcon;

    public VerticalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeEvents();
        InitializeDraggableHandler();
    }

    protected override void HandleDraggableRegionPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(draggableRectangle);
        var dimension = position.Y;
        var dimensionLength = draggableRectangleCanvas.Bounds.Height;
        var height = draggableRectangle.Height;
        var centerOffset = height / 2;
        var end = height;
        HandleDraggable(e, dimension, dimensionLength, centerOffset, end);
    }

    protected override void HandleDragging(PointerDragHandler.PointerDragArgs args)
    {
        var step = TranslateHeightToStep(args.TotalDelta.Y);
        Step(step);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = -e.Delta.Y * SmallStep;
        Step(delta);
    }

    protected override void OnUpdateScroll()
    {
        var availableHeight = draggableRectangleCanvas.Bounds.Height;
        var valueRange = ValidValueRange;
        var window = ScrollWindowLength;
        var start = DisplayStartPosition - MinValue;

        Canvas.SetTop(draggableRectangle, PixelValue(start));
        draggableRectangle.Height = PixelValue(window);

        double PixelValue(double scrollValue)
        {
            if (valueRange is 0)
                return 0;
            return scrollValue / valueRange * availableHeight;
        }
    }

    private double TranslateHeightToStep(double height)
    {
        return CalculateStep(height, draggableRectangleCanvas.Bounds.Height);
    }
}
