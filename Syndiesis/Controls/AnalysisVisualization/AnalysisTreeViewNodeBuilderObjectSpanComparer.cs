using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class AnalysisTreeViewNodeBuilderObjectSpanComparer
    : IComparer<UIBuilder.AnalysisTreeListNode>
{
    public static readonly AnalysisTreeViewNodeBuilderObjectSpanComparer Instance = new();

    public int Compare(UIBuilder.AnalysisTreeListNode? x, UIBuilder.AnalysisTreeListNode? y)
    {
        ArgumentNullException.ThrowIfNull(x, nameof(x));
        ArgumentNullException.ThrowIfNull(y, nameof(y));

        Debug.Assert(x.AssociatedSyntaxObject is not null);
        Debug.Assert(y.AssociatedSyntaxObject is not null);

        var xObject = x.AssociatedSyntaxObject!.Span;
        var yObject = y.AssociatedSyntaxObject!.Span;
        return xObject.CompareTo(yObject);
    }
}
