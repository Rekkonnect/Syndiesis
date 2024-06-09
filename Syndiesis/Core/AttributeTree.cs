using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Syndiesis.Core;

public abstract class AttributeTree(
    SyntaxTree tree,
    ImmutableArray<AttributeTree.SymbolContainer> containers)
{
    public SyntaxTree SyntaxTree { get; } = tree;
    public ImmutableArray<SymbolContainer> Containers { get; } = containers;

    public static AttributeTree? FromTree(
        Compilation compilation, SyntaxTree tree, CancellationToken token)
    {
        return tree switch
        {
            CSharpSyntaxTree csTree
                => CSharpAttributeTree.FromTree(compilation, csTree, token),
            VisualBasicSyntaxTree vbTree
                => VisualBasicAttributeTree.FromTree(compilation, vbTree, token),
            _ => null,
        };
    }

    protected abstract class Discoverer
    {
        public abstract int AttributeRawKind { get; }

        public abstract SyntaxNode? GetAttributedSymbolNode(SyntaxNode node);

        public ImmutableArray<SymbolContainer> DiscoverRootSymbols(
            Compilation compilation, SyntaxTree tree, CancellationToken cancellationToken)
        {
            var semanticModel = compilation.GetSemanticModel(tree, false);
            var syntaxRoot = tree.GetRoot(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return [];

            var attributes = ImmutableArray.CreateBuilder<SymbolContainer>();

            int attributeRawKind = AttributeRawKind;
            var nodes = syntaxRoot.DescendantNodes()
                .Where(s => s.RawKind == attributeRawKind);

            var attributedNodes = new HashSet<SyntaxNode>();

            foreach (var node in nodes)
            {
                var attributed = GetAttributedSymbolNode(node);
                if (attributed is null)
                    continue;

                attributedNodes.Add(attributed);
            }

            attributedNodes
                .Select(s => semanticModel.GetDeclaredSymbol(s, cancellationToken))
                .Where(static s => s is not null)
                .Select(s => new SymbolContainer(s!, s!.GetAttributes()))
                .ForEach(attributes.Add);

            return attributes.ToImmutable();
        }
    }

    public sealed record SymbolContainer(
        ISymbol Symbol, ImmutableArray<AttributeData> Attributes);
}
