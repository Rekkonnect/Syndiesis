using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Diagnostics.CodeAnalysis;

namespace CSharpSyntaxEditor.Controls;

public partial class VerticalScrollBar : BaseScrollBar
{
    private PointerDragHandler _dragHandler;

    public VerticalScrollBar()
    {
        InitializeComponent();
        InitializeBrushes();
        InitializeEvents();
    }

    private void InitializeBrushes()
    {
        upButton.HoverBackgroundBrush = AccentColorBrush;
        downButton.HoverBackgroundBrush = AccentColorBrush;

        upButton.Background = Background;
        downButton.Background = Background;

        upIcon.Fill = AccentColorBrush;
        downIcon.Fill = AccentColorBrush;
        draggableRectangle.Fill = AccentColorBrush;
    }

    [MemberNotNull(nameof(_dragHandler))]
    private void InitializeEvents()
    {
        upButton.Click += HandleUpClick;
        downButton.Click += HandleDownClick;
        draggableRectangle.PointerEntered += HandlePointerOverRectangle;
        draggableRectangle.PointerExited += HandlePointerOverRectangle;
        draggableRectangle.PointerMoved += HandlePointerOverRectangle;

        _dragHandler = new PointerDragHandler();
        _dragHandler.Dragged += HandleDragging;
        _dragHandler.Attach(draggableRectangle);
    }

    private void HandlePointerOverRectangle(object? sender, PointerEventArgs e)
    {
        bool isHovered = draggableRectangle.IsPointerOver || _dragHandler.IsActivelyDragging;
        var brush = BrushForHoverState(isHovered);
        draggableRectangle.Fill = brush;
    }

    private void HandleDragging(PointerDragHandler.PointerDragArgs args)
    {
        var heightStep = args.Delta.Y;
        var progressStep = heightStep / draggableRectangleCanvas.Bounds.Height;
        var translatedStep = progressStep * ValidValueRange;
        Step(translatedStep);
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

        var availableHeight = draggableRectangleCanvas.Bounds.Height;
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
}
