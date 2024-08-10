using Garyon.Functions;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

using static NodeDetailsViewData;

public sealed record NodeDetailsViewData(
    CurrentNodeSection CurrentNode,
    ParentNodeSection ParentNode,
    ChildrenSection Children,
    SemanticModelSection SemanticModel
    )
{
    private IList<UIBuilder.AnalysisTreeListNode> AllNodes()
    {
        return
        [
            CurrentNode.CurrentNode,
            CurrentNode.CurrentToken,
            CurrentNode.CurrentTrivia,

            ParentNode.ParentNode,
            ParentNode.ParentTrivia,

            Children.ChildNodes,
            Children.ChildTokens,
            Children.ChildNodesAndTokens,

            SemanticModel.SymbolInfo,
            SemanticModel.DeclaredSymbolInfo,
            SemanticModel.TypeInfo,
            SemanticModel.AliasInfo,
            SemanticModel.PreprocessingSymbolInfo,
            SemanticModel.Conversion,
            SemanticModel.Operation,
        ];
    }

    public async Task<bool> AwaitAllLoaded()
    {
        var nodeLoaders = AllNodes()
            .Select(s => s.NodeLoader)
            .Where(Predicates.NotNull)
            .ToList()
            ;

        await Task.WhenAll(nodeLoaders!);
        return nodeLoaders.All(l => l!.IsCompletedSuccessfully);
    }

    public sealed record CurrentNodeSection(
        UIBuilder.AnalysisTreeListNode CurrentNode,
        UIBuilder.AnalysisTreeListNode CurrentToken,
        UIBuilder.AnalysisTreeListNode CurrentTrivia
        );

    public sealed record ParentNodeSection(
        UIBuilder.AnalysisTreeListNode ParentNode,
        UIBuilder.AnalysisTreeListNode ParentTrivia
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
