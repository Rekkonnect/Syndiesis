using Microsoft.CodeAnalysis;
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
}
