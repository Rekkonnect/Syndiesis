using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace CSharpSyntaxEditor.Controls;

public partial class ScrollBarStepButtonContainer : UserControl
{
    public IBrush HoverBackgroundBrush { get; set; } = new SolidColorBrush(Colors.Transparent);
    public IBrush? ContentFillBrush { get; set; }
    public IBrush? HoverContentFillBrush { get; set; }

    public object? ContainerContent
    {
        get => contentControl.Content;
        set => contentControl.Content = value;
    }

    public ScrollBarStepButtonContainer()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        // should not be necessary?
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateContentBrush();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdateContentBrush();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateContentBrush();
    }

    private void UpdateContentBrush()
    {
        var content = contentControl.Content;
        if (content is null)
            return;

        if (ContentFillBrush is null)
            return;

        bool isHovered = IsPointerOver;

        if (content is Shape shape)
        {
            var fill = isHovered
                ? HoverContentFillBrush
                : ContentFillBrush;
            shape.SetCurrentValue(Shape.FillProperty, fill);
        }
    }
}
