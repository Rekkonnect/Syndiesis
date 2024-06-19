using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoseLynn.CSharp;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed record SyntaxObjectInfo(
    object SyntaxObject, TextSpan Span, TextSpan FullSpan)
{
    public static readonly TextSpan InvalidTextSpan = new(0, int.MaxValue - 5584);

    public SyntaxTree? SyntaxTree => GetSyntaxTree(SyntaxObject);

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
        if (span == InvalidTextSpan)
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

            case SyntaxReference reference:
                return reference.SyntaxTree;

            case VirtualTree virtualTree:
                return virtualTree.SyntaxTree;

            // Attribute
            case AttributeData attribute:
                return attribute.GetAttributeApplicationSyntax()?.SyntaxTree;

            // Operation
            case IOperation operation:
                return operation.Syntax.SyntaxTree;

            // Semantic model
            case SemanticModel semanticModel:
                return semanticModel.SyntaxTree;

            // Symbol
            case ISymbol symbol:
                return SymbolDeclaringSyntax(symbol)?.SyntaxTree;

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

            case SyntaxReference reference:
                return reference.Span;

            case VirtualTree virtualTree:
                return GetSpan(virtualTree.SyntaxTree);

            case TextSpan span:
                return span;

            // Attribute
            case AttributeData attribute:
                return attribute.ApplicationSyntaxReference
                    ?.Span ?? InvalidTextSpan;

            // Operation
            case IOperation operation:
                return operation.Syntax.Span;

            // Semantic model
            case SemanticModel semanticModel:
                return GetSpan(semanticModel.SyntaxTree);

            // Symbol
            case ISymbol symbol:
                // NOTE: This will not work well for partial declarations.
                // Partial declarations could have multiple such references, and thus
                // result in inaccurate behavior when interacting with the code.
                return SymbolDeclaringSyntax(symbol)
                    ?.GetSyntax().Span ?? InvalidTextSpan;
        }

        return InvalidTextSpan;
    }

    private static TextSpan GetFullSpan(object x)
    {
        switch (x)
        {
            // Syntax
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

            case SyntaxReference reference:
                return reference.GetSyntax().FullSpan;

            case VirtualTree virtualTree:
                return GetFullSpan(virtualTree.SyntaxTree);

            case TextSpan span:
                return span;

            // Attribute
            case AttributeData attribute:
                return attribute.GetAttributeApplicationSyntax()
                    ?.FullSpan ?? InvalidTextSpan;

            // Operation
            case IOperation operation:
                return operation.Syntax.FullSpan;

            // Semantic model
            case SemanticModel semanticModel:
                return GetFullSpan(semanticModel.SyntaxTree);

            // Symbol
            case ISymbol symbol:
                // NOTE: This will not work well for partial declarations.
                // Partial declarations could have multiple such references, and thus
                // result in inaccurate behavior when interacting with the code.
                return SymbolDeclaringSyntax(symbol)
                    ?.GetSyntax().FullSpan ?? InvalidTextSpan;
        }

        return InvalidTextSpan;
    }

    private static TextSpan ExtractSpanFromList<T>(
        IReadOnlyList<T?> nodeList,
        Func<object, TextSpan> spanGetter)
    {
        if (nodeList.IsEmpty())
            return InvalidTextSpan;

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

    private static SyntaxReference? SymbolDeclaringSyntax(ISymbol symbol)
    {
        if (symbol.IsImplicitlyDeclared)
        {
            if (symbol is not INamespaceSymbol { IsGlobalNamespace: true })
                return null;
        }

        return symbol.DeclaringSyntaxReferences.FirstOrDefault();
    }
}
