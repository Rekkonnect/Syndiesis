using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Syndiesis.Core;

public sealed class OperationTree(
    SyntaxTree tree,
    ImmutableArray<IOperation> operations)
{
    public SyntaxTree SyntaxTree { get; } = tree;
    public ImmutableArray<IOperation> Operations { get; } = operations;

    public static OperationTree? FromTree(
        Compilation compilation, SyntaxTree tree, CancellationToken token)
    {
        var operations = DiscoverRootOperations(compilation, tree, token);
        if (token.IsCancellationRequested)
            return null;

        return new(tree, operations);
    }

    private static ImmutableArray<IOperation> DiscoverRootOperations(
        Compilation compilation, SyntaxTree tree, CancellationToken cancellationToken)
    {
        var semanticModel = compilation.GetSemanticModel(tree, false);
        var syntaxRoot = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return [];

        var operations = ImmutableArray.CreateBuilder<IOperation>();

        var nodeQueue = new Queue<SyntaxNode>();
        nodeQueue.Enqueue(syntaxRoot);

        while (nodeQueue.Count > 0)
        {
            var node = nodeQueue.Dequeue();
            var operation = semanticModel.GetOperation(node, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return [];

            if (operation is not null)
            {
                operations.Add(operation);
                continue;
            }

            var children = node.ChildNodes();
            nodeQueue.EnqueueRange(children);
        }

        return operations.ToImmutable();
    }
}
