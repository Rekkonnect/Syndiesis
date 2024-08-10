using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
        var rootNode = syntaxTree.SyntaxNodeAtSpanIncludingStructuredTrivia(span);
        if (rootNode is null)
            return null;

        var token = rootNode.DeepestTokenContainingSpan(span);
        var trivia = rootNode.DeepestTriviaContainingSpan(span);
        return new(syntaxTree, rootNode, token, trivia);
    }
}
