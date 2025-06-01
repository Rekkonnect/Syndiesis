using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.Contracts;

namespace Syndiesis.Core;

public static class NodeViewAnalysisHelpers
{
    public static NodeViewAnalysisExecution? GetNodeViewAnalysisExecutionForSpan(
        HybridSingleTreeCompilationSource compilationSource,
        TextSpan span)
    {
        var analysisRoot = GetNodeViewAnalysisRootForSpan(compilationSource, span);
        if (analysisRoot is null)
            return null;

        var compilation = compilationSource.CurrentSource.Compilation!;
        return new(compilation, analysisRoot);
    }

    public static NodeViewAnalysisRoot? GetNodeViewAnalysisRootForSpan(
        HybridSingleTreeCompilationSource compilationSource,
        TextSpan span)
    {
        var tree = compilationSource.CurrentSource.Tree;
        if (tree is null)
            return null;

        return GetNodeViewAnalysisRootForSpan(tree, span);
    }

    public static NodeViewAnalysisRoot? GetNodeViewAnalysisRootForSpan(
        SyntaxTree syntaxTree,
        TextSpan span)
    {
        if (syntaxTree.Length is 0)
        {
            return GetNodeViewAnalysisRootForEmptySyntaxTree(syntaxTree);
        }

        var rootNode = syntaxTree.SyntaxNodeAtSpanIncludingStructuredTrivia(span);
        if (rootNode is null)
            return null;

        var token = rootNode.DeepestTokenContainingSpan(span);
        var trivia = rootNode.DeepestTriviaContainingSpan(span);
        return new(syntaxTree, rootNode, token, trivia);
    }

    private static NodeViewAnalysisRoot? GetNodeViewAnalysisRootForEmptySyntaxTree(
        SyntaxTree syntaxTree)
    {
        Contract.Assert(syntaxTree.Length is 0);

        var rootNode = syntaxTree.GetRoot();
        var token = rootNode.GetFirstToken();
        var trivia = rootNode.GetTrailingTrivia().FirstOrDefault();
        return new(syntaxTree, rootNode, token, trivia);
    }
}
