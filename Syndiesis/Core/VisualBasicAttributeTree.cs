using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Syndiesis.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

namespace Syndiesis.Core;

public sealed class VisualBasicAttributeTree(
    VisualBasicSyntaxTree vbTree,
    ImmutableArray<AttributeTree.SymbolContainer> containers)
    : AttributeTree(vbTree, containers)
{
    public static VisualBasicAttributeTree? FromTree(
        Compilation compilation, VisualBasicSyntaxTree tree, CancellationToken token)
    {
        var discoverer = Singleton<VisualBasicDiscoverer>.Instance;
        var attributeTree = discoverer.DiscoverRootSymbols(compilation, tree, token);
        if (token.IsCancellationRequested)
            return null;

        return new(tree, attributeTree);
    }

    private sealed class VisualBasicDiscoverer : Discoverer
    {
        public override int AttributeRawKind => (int)SyntaxKind.Attribute;

        public override SyntaxNode? GetAttributedSymbolNode(SyntaxNode node)
        {
            Debug.Assert(node is AttributeSyntax);
            return node
                !.Parent // AttributeList
                !.Parent // Attributed declaration
                ;
        }
    }
}
