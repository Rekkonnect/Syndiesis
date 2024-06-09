using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using RoseLynn;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Syndiesis.Core;

public abstract class AttributeTree(
    SyntaxTree tree,
    ImmutableArray<AttributeTree.SymbolContainer> containers)
    : VirtualTree(tree)
{
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

            var attributeContainers = ImmutableArray.CreateBuilder<SymbolContainer>();

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

            var attributedSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            foreach (var node in attributedNodes)
            {
                var declaration = semanticModel.GetDeclaredSymbol(node, cancellationToken);
                if (declaration is null)
                {
                    continue;
                }

                attributedSymbols.Add(declaration);
                var possible = GetPossiblyAttributedSymbols(declaration);
                foreach (var symbol in possible)
                {
                    attributedSymbols.Add(symbol);
                }
            }

            foreach (var symbol in attributedSymbols)
            {
                var directAttributes = GetAllDirectAttributes(symbol);
                CreateSymbolContainerAdd(symbol, directAttributes);
            }

            var compilationAssembly = compilation.Assembly;
            var compilationModule = compilation.SourceModule;

            CreateSymbolContainerAdd(
                compilationAssembly,
                compilationAssembly.GetAttributes());

            CreateSymbolContainerAdd(
                compilationModule,
                compilationModule.GetAttributes());

            return attributeContainers.ToImmutable();

            void CreateSymbolContainerAdd(
                ISymbol symbol,
                ImmutableArray<AttributeData> attributes)
            {
                if (attributes.Length is 0)
                    return;

                var container = new SymbolContainer(symbol, attributes);
                attributeContainers.Add(container);
            }
        }

        private static IReadOnlyList<ISymbol> GetPossiblyAttributedSymbols(ISymbol symbol)
        {
            switch (symbol)
            {
                // these handle the `param` attribute target for C#
                case IMethodSymbol
                {
                    MethodKind: MethodKind.PropertySet
                        or MethodKind.EventRemove
                        or MethodKind.EventAdd
                } accessor:
                    return accessor.Parameters;

                // this handles the `field` attribute target for C#
                case IPropertySymbol property:
                {
                    var field = AssociatedField(property);
                    if (field is not null)
                        return [field];

                    return [];
                }

                // this handles the `field` and `method` attribute targets for C#
                case IEventSymbol @event:
                {
                    var symbols = ImmutableArray.CreateBuilder<ISymbol>();

                    if (@event.AddMethod is not null and var add)
                        symbols.Add(add);

                    if (@event.RemoveMethod is not null and var remove)
                        symbols.Add(remove);

                    if (@event.RaiseMethod is not null and var raise)
                        symbols.Add(raise);

                    var field = AssociatedField(@event);
                    if (field is not null)
                        symbols.Add(field);

                    return symbols.ToImmutable();
                }
            }

            return [];
        }

        private static ImmutableArray<AttributeData> GetAllDirectAttributes(ISymbol symbol)
        {
            if (symbol is IMethodSymbol method)
            {
                return [
                    .. method.GetAttributes(),
                    .. method.GetReturnTypeAttributes(),
                ];
            }

            if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Delegate } delegateType)
            {
                return [
                    .. delegateType.GetAttributes(),
                    .. delegateType.DelegateInvokeMethod?.GetReturnTypeAttributes() ?? [],
                ];
            }

            return symbol.GetAttributes();
        }

        private static IFieldSymbol? AssociatedField(ISymbol symbol)
        {
            return symbol.ContainingType
                .GetMembers<IFieldSymbol>()
                .FirstOrDefault(s
                    => SymbolEqualityComparer.Default.Equals(s.AssociatedSymbol, symbol))
                ;
        }
    }

    public sealed record SymbolContainer(
        ISymbol Symbol, ImmutableArray<AttributeData> Attributes);
}
