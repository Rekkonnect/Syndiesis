using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Threading;

namespace Syndiesis.Core;

public static class SyntaxExtensions
{
    public static bool HasAnyTrivia(this SyntaxNode node)
    {
        return node.HasLeadingTrivia
            || node.HasTrailingTrivia;
    }

    public static bool HasAnyTrivia(this SyntaxToken token)
    {
        return token.HasLeadingTrivia
            || token.HasTrailingTrivia;
    }

    public static bool IsEmpty(this SyntaxToken token)
    {
        return token.Span.IsEmpty;
    }

    public static bool IsFullEmpty(this SyntaxToken token)
    {
        return token.FullSpan.IsEmpty;
    }

    public static string? GetLanguage(
        this SyntaxTree tree,
        CancellationToken cancellationToken = default)
    {
        return tree.GetRoot(cancellationToken)?.Language;
    }

    public static IEnumerable<SyntaxNode> EnumerateAncestorsWithSameSpanAndThis(this SyntaxNode node)
    {
        yield return node;

        var span = node.Span;
        var current = node;
        while (true)
        {
            var parent = current.Parent;
            if (parent is null)
                yield break;

            if (parent.Span != span)
                yield break;
            
            yield return parent;
            current = parent;
        }
    }
}
