using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Syndiesis.Core;

public sealed class OperationTree(
    SyntaxTree tree,
    ImmutableArray<OperationTree.SymbolContainer> operations)
{
    public SyntaxTree SyntaxTree { get; } = tree;
    public ImmutableArray<SymbolContainer> Containers { get; } = operations;

    public static OperationTree? FromTree(
        Compilation compilation, SyntaxTree tree, CancellationToken token)
    {
        var operations = DiscoverRootOperations(compilation, tree, token);
        if (token.IsCancellationRequested)
            return null;

        return new(tree, operations);
    }

    private static ImmutableArray<SymbolContainer> DiscoverRootOperations(
        Compilation compilation, SyntaxTree tree, CancellationToken cancellationToken)
    {
        var semanticModel = compilation.GetSemanticModel(tree, false);
        var syntaxRoot = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return [];

        var operations = ImmutableArray.CreateBuilder<SymbolOperation>();

        var nodeQueue = new Queue<SyntaxNode>();
        nodeQueue.Enqueue(syntaxRoot);

        while (nodeQueue.Count > 0)
        {
            var node = nodeQueue.Dequeue();
            var operation = semanticModel.GetOperation(node, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return [];

            if (operation is not null and not IAttributeOperation)
            {
                var symbol = semanticModel.GetEnclosingSymbol(node, cancellationToken);
                // we must have a symbol here
                Debug.Assert(symbol is not null);
                operations.Add(new (symbol, operation));
                continue;
            }

            var children = node.ChildNodes();
            nodeQueue.EnqueueRange(children);
        }

        return operations
            .GroupBy(s => s.Symbol, SymbolEqualityComparer.Default)
            .Select(s => new SymbolContainer(s.Key, s.Select(o => o.Operation).ToImmutableArray()))
            .ToImmutableArray()
            ;
    }

    public sealed record SymbolContainer(
        ISymbol Symbol, ImmutableArray<IOperation> Operations);

    private readonly record struct SymbolOperation(
        ISymbol Symbol, IOperation Operation);
}
