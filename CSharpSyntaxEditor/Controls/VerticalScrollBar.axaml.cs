using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace CSharpSyntaxEditor.Controls;

public partial class VerticalScrollBar : BaseScrollBar
{
    public VerticalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeEvents();
    }

    private void InitializeBrushes()
    {
        //upButton.HoverBackgroundBrush = AccentColorBrush;
        //downButton.HoverBackgroundBrush = AccentColorBrush;

        //upButton.Background = Background;
        //downButton.Background = Background;

        //upIcon.Fill = AccentColorBrush;
        //downIcon.Fill = AccentColorBrush;
        //draggableRectangle.Fill = AccentColorBrush;
    }

    private void InitializeEvents()
    {
        upButton.Click += HandleUpClick;
        downButton.Click += HandleDownClick;
    }

    private void HandleUpClick(object? sender, RoutedEventArgs e)
    {
        Step(-SmallStep);
    }

    private void HandleDownClick(object? sender, RoutedEventArgs e)
    {
        Step(SmallStep);
    }

    protected override void OnUpdateScroll()
    {
        draggableRectangle.IsVisible = HasAvailableScroll;

        if (!HasAvailableScroll)
        {
            return;
        }

        var availableHeight = draggableRectangleCanvas.Height;
        var valueRange = ValidValueRange;
        var window = ScrollWindowLength;
        var start = StartPosition;

        Canvas.SetTop(draggableRectangle, PixelValue(start));
        draggableRectangle.Height = PixelValue(window);

        double PixelValue(double scrollValue)
        {
            return scrollValue / valueRange * availableHeight;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdateHovers();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateHovers();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateHovers();
    }

    private void UpdateHovers()
    {
        SetHover(draggableRectangle, draggableRectangle);
    }

    private void SetHover(Control container, Shape fillable)
    {
        bool isHovered = container.IsPointerOver;
        var brush = BrushForHoverState(isHovered);
        fillable.Fill = brush;
    }
}
