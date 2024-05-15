using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
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
        var fullSpan = GetFullSpan(x);
        return new(x, span, fullSpan);
    }

    private static SyntaxTree? GetSyntaxTree(object? x)
    {
        switch (x)
        {
            case SyntaxNode node:
                return node.SyntaxTree;

            case SyntaxToken token:
                return token.SyntaxTree;

            case SyntaxTrivia trivia:
                return trivia.SyntaxTree;

            case IReadOnlyList<object?> nodeList:
                return GetSyntaxTree(nodeList.FirstOrDefault());

            case SyntaxTokenList tokenList:
                return tokenList.FirstOrDefault().SyntaxTree;

            case null:
                return null;
        }

        throw new ArgumentException("Unknown object to get FullSpan from");
    }

    private static TextSpan GetSpan(object x)
    {
        switch (x)
        {
            case SyntaxNode node:
                return node.Span;

            case SyntaxToken token:
                return token.Span;

            case SyntaxTrivia trivia:
                return trivia.Span;

            case IReadOnlyList<object?> nodeList:
                return ExtractSpanFromList(nodeList, GetSpan);

            case SyntaxTokenList tokenList:
                return ExtractSpanFromList(tokenList, GetSpan);
        }

        throw new ArgumentException("Unknown object to get Span from");
    }

    private static TextSpan GetFullSpan(object x)
    {
        switch (x)
        {
            case SyntaxNode node:
                return node.FullSpan;

            case SyntaxToken token:
                return token.FullSpan;

            case SyntaxTrivia trivia:
                return trivia.FullSpan;

            case IReadOnlyList<object?> nodeList:
                return ExtractSpanFromList(nodeList, GetFullSpan);

            case SyntaxTokenList tokenList:
                return ExtractSpanFromList(tokenList, GetFullSpan);
        }

        throw new ArgumentException("Unknown object to get FullSpan from");
    }

    private static TextSpan ExtractSpanFromList<T>(
        IReadOnlyList<T?> nodeList,
        Func<object, TextSpan> spanGetter)
    {
        if (nodeList.Count is 0)
            throw new ArgumentException("Invalid empty list provided");

        var first = nodeList[0];
        var firstSpan = spanGetter(first!);
        if (nodeList.Count is 1)
            return firstSpan;

        var start = firstSpan.Start;
        var last = nodeList[^1];
        var lastSpan = spanGetter(last!);
        var end = lastSpan.End;
        return TextSpan.FromBounds(start, end);
    }
}
