using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace Syndiesis.Controls;

public partial class VerticalScrollBar : BaseScrollBar
{
    public override ScrollBarStepButtonContainer PreviousButtonContainer => upButton;
    public override ScrollBarStepButtonContainer NextButtonContainer => downButton;
    public override Rectangle DraggableRectangle => draggableRectangle;

    public override Shape PreviousIconShape => upIcon;
    public override Shape NextIconShape => downIcon;

    public VerticalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeBrushesExtra();
        InitializeEvents();
        InitializeDraggableHandler();
    }

    private void InitializeBrushesExtra()
    {
        hitboxPanel.Background = Background;
    }

    protected override void HandleDragging(PointerDragHandler.PointerDragArgs args)
    {
        var step = CalculateStep(args.TotalDelta.Y, draggableRectangleCanvas.Bounds.Height);
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
}
