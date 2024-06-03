using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Threading;

namespace Syndiesis.Core;

public static class RoslynExtensions
{
    public static ISymbol? GetEnclosingSymbol(
        this SemanticModel model,
        SyntaxNode node,
        CancellationToken cancellationToken = default)
    {
        var position = node.SpanStart;
        return model.GetEnclosingSymbol(position, cancellationToken);
    }

    public static bool IsElastic(
        this SyntaxAnnotation annotation)
    {
        return annotation.Kind is null
            && annotation.Data is null
            ;
    }

    public static SyntaxToken DeepestTokenContainingPosition(this SyntaxNode parent, int position)
    {
        if (!parent.FullSpan.Contains(position))
            return default;

        var current = parent;

        while (true)
        {
            var child = current.ChildThatContainsPosition(position);
            if (child == default)
                return default;

            if (child.IsToken)
                return child.AsToken();

            var node = child.AsNode();
            Debug.Assert(node is not null);
            current = node;
        }
    }

    public static SyntaxNode? DeepestNodeContainingPosition(this SyntaxNode parent, int position)
    {
        if (!parent.FullSpan.Contains(position))
            return null;

        var current = parent;

        while (true)
        {
            var child = current.ChildThatContainsPosition(position);
            if (child == default)
                return current;

            if (child.IsToken)
                return current;

            var node = child.AsNode();
            Debug.Assert(node is not null);
            current = node;
        }
    }
}
