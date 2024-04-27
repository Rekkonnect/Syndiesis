using Avalonia.Controls;
using Syndiesis.Controls;
using System.Collections.Generic;

namespace Syndiesis.Views.DesignerPreviews;

public partial class SyntaxTreeListViewPreview : UserControl
{
    public SyntaxTreeListViewPreview()
    {
        InitializeComponent();
    }

    private void CorrectContainedNodeWidths()
    {
        var thisBounds = Bounds;
        var thisRight = thisBounds.Right;

        foreach (var childNode in EnumerateNodes())
        {
            var childBounds = childNode.Bounds;
            var right = childBounds.Right;
            var missing = thisRight - right;
            if (missing > 0)
            {
                childNode.Width += missing;
            }
        }
    }

    private IEnumerable<SyntaxTreeListNode> EnumerateNodes()
    {
        return EnumerateNodes(rootNode);
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
