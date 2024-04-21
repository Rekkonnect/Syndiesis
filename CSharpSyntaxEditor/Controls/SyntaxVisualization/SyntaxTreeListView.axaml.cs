using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace CSharpSyntaxEditor.Controls;

public partial class SyntaxTreeListView : UserControl
{
    public static readonly StyledProperty<SyntaxTreeListNode> RootNodeProperty =
        AvaloniaProperty.Register<CodeEditorLine, SyntaxTreeListNode>(
            nameof(RootNode),
            defaultValue: new());

    public SyntaxTreeListNode RootNode
    {
        get => GetValue(RootNodeProperty);
        set
        {
            SetValue(RootNodeProperty, value);
            topLevelNodeContent.Content = value;
        }
    }

    public SyntaxTreeListView()
    {
        InitializeComponent();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        RootNode.EvaluateHoveringRecursively(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        CorrectContainedNodeWidths(availableSize);
        return base.MeasureOverride(availableSize);
    }

    private void CorrectContainedNodeWidths(Size availableSize)
    {
        RootNode.CorrectContainedNodeWidths(availableSize);
    }
}
