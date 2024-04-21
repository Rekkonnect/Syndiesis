using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.Generic;

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
        var thisBounds = Bounds;
        var newWidth = availableSize.Width;
        var thisLeft = thisBounds.Left;
        var newRight = newWidth + thisBounds.Left;

        foreach (var childNode in EnumerateNodes())
        {
            var childBounds = childNode.Bounds;
            // avoid breaking first init
            if (childBounds.Width is 0)
                continue;
            var right = childBounds.Right;
            var offset = childBounds.Left - thisLeft;
            var newChildWidth = newWidth - offset;
            var missing = newChildWidth - childBounds.Width;
            if (missing > 0)
            {
                childNode.Width = newChildWidth;
            }
        }
    }

    private IEnumerable<SyntaxTreeListNode> EnumerateNodes()
    {
        return EnumerateNodes(RootNode);
    }
    private static IEnumerable<SyntaxTreeListNode> EnumerateNodes(SyntaxTreeListNode parent)
    {
        yield return parent;

        foreach (var child in parent.ChildNodes)
        {
            var enumeratedNodes = EnumerateNodes(child);
            foreach (var node in enumeratedNodes)
            {
                yield return node;
            }
        }
    }
}
