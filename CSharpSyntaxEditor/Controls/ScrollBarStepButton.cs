using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace CSharpSyntaxEditor.Controls;

public class ScrollBarStepButton : Button
{
    public IBrush HoverBackgroundBrush { get; set; } = new SolidColorBrush(Colors.Transparent);
    public IBrush? ContentFillBrush { get; set; }

    public ScrollBarStepButton()
    {
        //InitializeComponent();
    }

    private void InitializeComponent()
    {
        CornerRadius = new(0);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateContentBrush();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateContentBrush();
    }

    private void UpdateContentBrush()
    {
        return;

        var content = Content;
        if (content is null)
            return;

        if (ContentFillBrush is null)
            return;

        bool isHovered = IsPointerOver;
        if (content is Shape shape)
        {
            var priority = isHovered
                ? BindingPriority.LocalValue
                : BindingPriority.Unset;
            shape.SetValue(Shape.FillProperty, ContentFillBrush, priority);
        }
    }
}
