using Microsoft.CodeAnalysis;

namespace Syndiesis.Core;

public abstract class VirtualTree(
    SyntaxTree tree)
{
    public SyntaxTree SyntaxTree { get; } = tree;
}
