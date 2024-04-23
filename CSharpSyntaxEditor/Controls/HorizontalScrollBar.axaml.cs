using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace CSharpSyntaxEditor.Controls;

public partial class HorizontalScrollBar : BaseScrollBar
{
    public override ScrollBarStepButtonContainer PreviousButtonContainer => leftButton;
    public override ScrollBarStepButtonContainer NextButtonContainer => rightButton;
    public override Rectangle DraggableRectangle => draggableRectangle;

    public override Shape PreviousIconShape => leftIcon;
    public override Shape NextIconShape => rightIcon;

    public HorizontalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeEvents();
        InitializeDraggableHandler();
    }

    protected override void HandleDragging(PointerDragHandler.PointerDragArgs args)
    {
        var widthStep = args.Delta.X;
        var progressStep = widthStep / draggableRectangleCanvas.Bounds.Width;
        var translatedStep = progressStep * ValidValueRange;
        Step(translatedStep);
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
        var start = StartPosition;

        Canvas.SetLeft(draggableRectangle, PixelValue(start));
        draggableRectangle.Width = PixelValue(window);

        double PixelValue(double scrollValue)
        {
            return scrollValue / valueRange * availableWidth;
        }
    }
}
