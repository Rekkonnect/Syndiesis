using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;

namespace Syndiesis.Core;

public sealed class CSharpAttributeTree(
    CSharpSyntaxTree csTree,
    ImmutableArray<AttributeTree.SymbolContainer> containers)
    : AttributeTree(csTree, containers)
{
    public static CSharpAttributeTree? FromTree(
        Compilation compilation, CSharpSyntaxTree tree, CancellationToken token)
    {
        var discoverer = Singleton<CSharpDiscoverer>.Instance;
        var attributeTree = discoverer.DiscoverRootSymbols(compilation, tree, token);
        if (token.IsCancellationRequested)
            return null;

        return new(tree, attributeTree);
    }

    private sealed class CSharpDiscoverer : Discoverer
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
