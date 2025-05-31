namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class AnalysisTreeViewNodeObjectSpanComparer : IComparer<AnalysisTreeListNode>
{
    public static readonly AnalysisTreeViewNodeObjectSpanComparer Instance = new();

    public int Compare(AnalysisTreeListNode? x, AnalysisTreeListNode? y)
    {
        ArgumentNullException.ThrowIfNull(x, nameof(x));
        ArgumentNullException.ThrowIfNull(y, nameof(y));

        var xObject = x.AssociatedSyntaxObject!.Span;
        var yObject = y.AssociatedSyntaxObject!.Span;
        return xObject.CompareTo(yObject);
    }
}
