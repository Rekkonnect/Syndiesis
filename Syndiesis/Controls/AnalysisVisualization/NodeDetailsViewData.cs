using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.AnalysisVisualization;

using static NodeDetailsViewData;

public sealed record NodeDetailsViewData(
    CurrentNodeSection CurrentNode,
    ParentNodeSection ParentNode,
    ChildrenSection Children,
    SemanticModelSection SemanticModel
    )
{
    public sealed record CurrentNodeSection(
        UIBuilder.AnalysisTreeListNode CurrentNode
        );

    public sealed record ParentNodeSection(
        UIBuilder.AnalysisTreeListNode ParentNode
        );

    public sealed record ChildrenSection(
        UIBuilder.AnalysisTreeListNode ChildNodes,
        UIBuilder.AnalysisTreeListNode ChildTokens,
        UIBuilder.AnalysisTreeListNode ChildNodesAndTokens
        );

    public sealed record SemanticModelSection(
        UIBuilder.AnalysisTreeListNode SymbolInfo,
        UIBuilder.AnalysisTreeListNode DeclaredSymbolInfo,
        UIBuilder.AnalysisTreeListNode TypeInfo,
        UIBuilder.AnalysisTreeListNode AliasInfo,
        UIBuilder.AnalysisTreeListNode PreprocessingSymbolInfo,
        UIBuilder.AnalysisTreeListNode Conversion,
        UIBuilder.AnalysisTreeListNode Operation
        );
}
