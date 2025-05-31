using Microsoft.CodeAnalysis;

namespace Syndiesis.Core;

/// <summary>
/// Represents a <see cref="Microsoft.CodeAnalysis.SyntaxTree"/> wrapper that
/// contains a view of the given source, usually for semantic information. Such
/// trees are constructed from Syndiesis' analysis, and are not returned from
/// public Roslyn APIs.
/// </summary>
public abstract class VirtualTree(
    SyntaxTree tree)
{
    public SyntaxTree SyntaxTree { get; } = tree;
}
