using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace CSharpSyntaxEditor.Controls;

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
        var heightStep = args.Delta.Y;
        var progressStep = heightStep / draggableRectangleCanvas.Bounds.Height;
        var translatedStep = progressStep * ValidValueRange;
        Step(translatedStep);
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
        var start = StartPosition - MinValue;

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
