using Syndiesis.Core.DisplayAnalysis;
using System;
using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class AnalysisTreeViewNodeBuilderObjectSpanComparer
    : IComparer<UIBuilder.AnalysisTreeListNode>
{
    public static readonly AnalysisTreeViewNodeBuilderObjectSpanComparer Instance = new();

    public int Compare(UIBuilder.AnalysisTreeListNode? x, UIBuilder.AnalysisTreeListNode? y)
    {
        ArgumentNullException.ThrowIfNull(x, nameof(x));
        ArgumentNullException.ThrowIfNull(y, nameof(y));

        var xObject = x.AssociatedSyntaxObject!.Span;
        var yObject = y.AssociatedSyntaxObject!.Span;
        return xObject.CompareTo(yObject);
    }
}
