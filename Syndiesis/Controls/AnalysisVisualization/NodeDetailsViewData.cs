using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Immutable;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed record NodeDetailsViewData(
    UIBuilder.AnalysisTreeListNode CurrentNode,
    UIBuilder.AnalysisTreeListNode ParentNode,
    ImmutableArray<UIBuilder.AnalysisTreeListNode> Properties);
