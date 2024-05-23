using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed record SyntaxObjectInfo(
    object SyntaxObject, TextSpan Span, TextSpan FullSpan)
{
    public SyntaxTree? SyntaxTree => GetSyntaxTree(SyntaxObject);

    public LinePositionSpan LineFullSpan => GetLineFullSpan(SyntaxTree);
    public LinePositionSpan LineSpan => GetLineSpan(SyntaxTree);

    public LinePositionSpan GetLineFullSpan(SyntaxTree? tree)
    {
        return LineSpanOrDefault(tree, FullSpan);
    }

    public LinePositionSpan GetLineSpan(SyntaxTree? tree)
    {
        return LineSpanOrDefault(tree, Span);
    }

    private static LinePositionSpan LineSpanOrDefault(SyntaxTree? tree, TextSpan span)
    {
        try
        {
            return tree?.GetLineSpan(span).Span ?? default;
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(ex,
                $"{nameof(LineSpanOrDefault)} threw unexpectedly");

#if DEBUG
            throw;
#else
            return default;
#endif
        }
    }

    public static SyntaxObjectInfo? GetInfoForObject(object? x)
    {
        if (x is null)
            return null;

        if (x is SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.AsNode()!;
                return GetInfoForObject(node);
            }
            else
            {
                var token = nodeOrToken.AsToken()!;
                return GetInfoForObject(token);
            }
        }

        var span = GetSpan(x);
        if (span == default)
            return null;

        var fullSpan = GetFullSpan(x);
        return new(x, span, fullSpan);
    }

    private static SyntaxTree? GetSyntaxTree(object? x)
    {
        switch (x)
        {
            // Syntax
            case SyntaxTree tree:
                return tree;

            case SyntaxNode node:
                return node.SyntaxTree;

            case SyntaxToken token:
                return token.SyntaxTree;

            case SyntaxTrivia trivia:
                return trivia.SyntaxTree;

            case SyntaxTriviaList triviaList:
                return triviaList.FirstOrDefault().SyntaxTree;

            case IReadOnlyList<object?> nodeList:
                return GetSyntaxTree(nodeList.FirstOrDefault());

            case SyntaxTokenList tokenList:
                return tokenList.FirstOrDefault().SyntaxTree;

            // Operation
            case IOperation operation:
                return operation.Syntax.SyntaxTree;

            case OperationTree operationTree:
                return operationTree.SyntaxTree;

            // Semantic model
            case SemanticModel semanticModel:
                return semanticModel.SyntaxTree;

            // Symbol
            case ISymbol symbol:
                return symbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree;

            case null:
                return null;
        }

        throw new ArgumentException("Unknown object to get SyntaxTree from");
    }

    private static TextSpan GetSpan(object x)
    {
        switch (x)
        {
            // Syntax
            case SyntaxTree tree:
                return tree.GetRoot().Span;

            case SyntaxNode node:
                return node.Span;

            case SyntaxToken token:
                return token.Span;

            case SyntaxTrivia trivia:
                return trivia.Span;

            case SyntaxTriviaList triviaList:
                return triviaList.Span;

            case IReadOnlyList<object?> nodeList:
                return ExtractSpanFromList(nodeList, GetSpan);

            case SyntaxTokenList tokenList:
                return ExtractSpanFromList(tokenList, GetSpan);

            // Operation
            case IOperation operation:
                return operation.Syntax.Span;

            case OperationTree operationTree:
                return GetSpan(operationTree.SyntaxTree);

            // Semantic model
            case SemanticModel semanticModel:
                return GetSpan(semanticModel.SyntaxTree);

            // Symbol
            case ISymbol symbol:
                // NOTE: This will not work well for partial declarations.
                // Partial declarations could have multiple such references, and thus
                // result in inaccurate behavior when interacting with the code.
                return symbol.DeclaringSyntaxReferences.FirstOrDefault()
                    ?.GetSyntax().Span ?? default;
        }

        return default;
    }

    private static TextSpan GetFullSpan(object x)
    {
        switch (x)
        {
            // Operation
            case SyntaxTree tree:
                return tree.GetRoot().FullSpan;

            case SyntaxNode node:
                return node.FullSpan;

            case SyntaxToken token:
                return token.FullSpan;

            case SyntaxTrivia trivia:
                return trivia.FullSpan;

            case SyntaxTriviaList triviaList:
                return triviaList.FullSpan;

            case IReadOnlyList<object?> nodeList:
                return ExtractSpanFromList(nodeList, GetFullSpan);

            case SyntaxTokenList tokenList:
                return ExtractSpanFromList(tokenList, GetFullSpan);

            // Operation
            case IOperation operation:
                return operation.Syntax.FullSpan;

            case OperationTree operationTree:
                return GetFullSpan(operationTree.SyntaxTree);

            // Semantic model
            case SemanticModel semanticModel:
                return GetFullSpan(semanticModel.SyntaxTree);

            // Symbol
            case ISymbol symbol:
                // NOTE: This will not work well for partial declarations.
                // Partial declarations could have multiple such references, and thus
                // result in inaccurate behavior when interacting with the code.
                return symbol.DeclaringSyntaxReferences.FirstOrDefault()
                    ?.GetSyntax().FullSpan ?? default;
        }

        return default;
    }

    private static TextSpan ExtractSpanFromList<T>(
        IReadOnlyList<T?> nodeList,
        Func<object, TextSpan> spanGetter)
    {
        if (nodeList.IsEmpty())
            return default;

        var first = nodeList[0];
        var firstSpan = spanGetter(first!);
        if (nodeList.Count is 1)
            return firstSpan;

        if (first is ISymbol symbol)
        {
            return Symbol();

            TextSpan Symbol()
            {
                bool hasValid = !firstSpan.IsEmpty;
                var start = firstSpan.Start;
                var end = firstSpan.End;

                int count = nodeList.Count;
                for (int i = 1; i < count; i++)
                {
                    var node = nodeList[i]!;
                    var span = spanGetter(node);
                    if (span.IsEmpty)
                        continue;

                    if (!hasValid)
                    {
                        start = span.Start;
                        end = span.End;
                        hasValid = true;
                        continue;
                    }

                    start = Math.Min(start, span.Start);
                    end = Math.Max(end, span.End);
                }

                return TextSpan.FromBounds(start, end);
            }
        }

        return General();

        TextSpan General()
        {
            var start = firstSpan.Start;
            var last = nodeList[^1];
            var lastSpan = spanGetter(last!);
            var end = lastSpan.End;
            return TextSpan.FromBounds(start, end);
        }
    }
}
