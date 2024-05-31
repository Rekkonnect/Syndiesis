using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Syndiesis.Controls.Editor;

public sealed partial class CSharpRoslynColorizer(SingleTreeCompilationSource compilationSource)
    : RoslynColorizer(compilationSource)
{
    private readonly DocumentLineDictionary<CancellationTokenFactory> _lineCancellations = new();

    protected override void ColorizeLine(DocumentLine line)
    {
        var cancellationTokenFactory = _lineCancellations.GetOrAdd(
            line,
            static () => new CancellationTokenFactory());

        cancellationTokenFactory.Cancel();

        PerformColorization(line, cancellationTokenFactory.CurrentToken);
    }

    private void PerformColorization(
        DocumentLine line, CancellationToken cancellationToken)
    {
        var source = CompilationSource;
        var tree = source.Tree as CSharpSyntaxTree;
        var model = source.SemanticModel;
        if (tree is not null)
        {
            InitiateSyntaxColorization(line, tree, cancellationToken);
        }
        if (model is not null)
        {
            InitiateSemanticColorization(line, model, cancellationToken);
        }
    }

    private void InitiateSyntaxColorization(
        DocumentLine line, CSharpSyntaxTree tree, CancellationToken cancellationToken)
    {
        int offset = line.Offset;
        int endOffset = line.EndOffset;
        var root = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        var startNode = DeepestNodeContainingPosition(root, offset)!;
        var endNode = DeepestNodeContainingPosition(root, endOffset)!;

        var parent = CommonParent(startNode, endNode);

        var descendants = parent.DescendantTokens();

        foreach (var token in descendants)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var symKind = GetDeclaringSymbolKind(token);
            var colorizer = GetColorizer(symKind);
            if (colorizer is null)
                continue;

            var span = token.Span;
            ChangeLinePart(span.Start, span.End, colorizer);
        }
    }

    private void InitiateSemanticColorization(
        DocumentLine line, SemanticModel model, CancellationToken cancellationToken)
    {
        var tree = model.SyntaxTree;
        int offset = line.Offset;
        int endOffset = line.EndOffset;
        var root = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        var startNode = DeepestNodeContainingPosition(root, offset)!;
        var endNode = DeepestNodeContainingPosition(root, endOffset)!;

        var parent = CommonParent(startNode, endNode);

        var descendantIdentifiers = parent
            .DescendantTokens(static t => t.IsKind(SyntaxKind.IdentifierToken));

        foreach (var identifier in descendantIdentifiers)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var symbolInfo = model.GetSymbolInfo(identifier.Parent!, cancellationToken);
            var colorizer = GetColorizer(symbolInfo);
            if (colorizer is null)
                continue;

            var span = identifier.Span;
            ChangeLinePart(span.Start, span.End, colorizer);
        }
    }

    private Action<VisualLineElement>? GetColorizer(
        SymbolInfo symbolInfo)
    {
        var symbol = symbolInfo.Symbol;
        if (symbol is null)
            return null;

        var kind = symbol.Kind;
        switch (kind)
        {
            case SymbolKind.NamedType:
                return GetColorizer(((INamedTypeSymbol)symbol).TypeKind);
        }

        return GetColorizer(kind);
    }

    private Action<VisualLineElement>? GetColorizer(SymbolKind kind)
    {
        var brush = BrushForSymbolKind(kind);
        return ColorizerForBrush(brush);
    }

    private Action<VisualLineElement>? GetColorizer(TypeKind kind)
    {
        var brush = BrushForTypeKind(kind);
        return ColorizerForBrush(brush);
    }

    private Action<VisualLineElement>? ColorizerForBrush(IBrush? brush)
    {
        if (brush is not null)
        {
            return e => FormatWith(e, brush);
        }

        return null;
    }

    private Action<VisualLineElement>? GetColorizer(SymbolTypeKind kind)
    {
        var symbolKind = kind.SymbolKind;
        if (symbolKind is SymbolKind.NamedType)
        {
            return GetColorizer(kind.TypeKind);
        }

        return GetColorizer(symbolKind);
    }

    private static IBrush? BrushForSymbolKind(SymbolKind kind)
    {
        return kind switch
        {
            SymbolKind.Field => Styles.FieldForeground,
            SymbolKind.Property => Styles.PropertyForeground,
            SymbolKind.Event => Styles.EventForeground,
            SymbolKind.Method => Styles.MethodForeground,
            SymbolKind.Local => Styles.LocalForeground,
            SymbolKind.Label => Styles.LabelForeground,
            SymbolKind.RangeVariable => Styles.RangeVariableForeground,
            SymbolKind.Preprocessing => Styles.PreprocessingForeground,
            SymbolKind.TypeParameter => Styles.TypeParameterForeground,
            _ => null,
        };
    }

    private static IBrush? BrushForTypeKind(TypeKind kind)
    {
        return kind switch
        {
            TypeKind.Class => Styles.TypeParameterForeground,
            TypeKind.Struct => Styles.StructForeground,
            TypeKind.Interface => Styles.InterfaceForeground,
            TypeKind.Delegate => Styles.DelegateForeground,
            TypeKind.Enum => Styles.EnumForeground,
            TypeKind.TypeParameter => Styles.TypeParameterForeground,
            _ => null,
        };
    }

    private static SymbolTypeKind GetDeclaringSymbolKind(SyntaxToken token)
    {
        Debug.Assert(token.Kind() is SyntaxKind.IdentifierToken);
        var parent = token.Parent!;
        switch (parent)
        {
            case ClassDeclarationSyntax classDeclaration
            when classDeclaration.Identifier.Span == token.Span:
                return TypeKind.Class;
            case StructDeclarationSyntax structDeclaration
            when structDeclaration.Identifier.Span == token.Span:
                return TypeKind.Struct;
            case InterfaceDeclarationSyntax interfaceDeclaration
            when interfaceDeclaration.Identifier.Span == token.Span:
                return TypeKind.Interface;
            case DelegateDeclarationSyntax delegateDeclaration
            when delegateDeclaration.Identifier.Span == token.Span:
                return TypeKind.Delegate;
            case EnumDeclarationSyntax enumDeclaration
            when enumDeclaration.Identifier.Span == token.Span:
                return TypeKind.Enum;
            case RecordDeclarationSyntax recordDeclaration
            when recordDeclaration.Identifier.Span == token.Span:
                return recordDeclaration.ClassOrStructKeyword.Kind() switch
                {
                    SyntaxKind.ClassKeyword => TypeKind.Class,
                    SyntaxKind.StructKeyword => TypeKind.Struct,
                    _ => throw new UnreachableException(),
                };

            case TypeParameterSyntax typeParameter
            when typeParameter.Identifier.Span == token.Span:
                return SymbolTypeKind.TypeParameter;

            case ParameterSyntax parameterSyntax
            when parameterSyntax.Identifier.Span == token.Span:
                return SymbolKind.Parameter;

            case VariableDeclaratorSyntax variableDeclarator
            when variableDeclarator.Identifier.Span == token.Span:
            {
                var declaration = variableDeclarator.Parent as VariableDeclarationSyntax;
                var container = declaration!.Parent;
                return container switch
                {
                    LocalDeclarationStatementSyntax => SymbolKind.Local,
                    FieldDeclarationSyntax => SymbolKind.Field,
                    EventFieldDeclarationSyntax => SymbolKind.Event,
                    _ => throw new UnreachableException("what else could be contained here?"),
                };
            }

            case PropertyDeclarationSyntax propertyDeclaration
            when propertyDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Property;

            case EventDeclarationSyntax eventDeclaration
            when eventDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Event;

            case MethodDeclarationSyntax methodDeclaration
            when methodDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Method;
        }

        return default;
    }

    private void FormatWith(VisualLineElement element, IBrush brush)
    {
        element.TextRunProperties.SetForegroundBrush(brush);
    }

    private SyntaxNode CommonParent(SyntaxNode a, SyntaxNode b)
    {
        var current = a;
        var bSpan = b.Span;

        while (true)
        {
            // a is always contained, since we traverse the ancestors of a
            if (current.Span.Contains(bSpan))
            {
                return current;
            }

            var parent = current.Parent;
            Debug.Assert(
                parent is not null,
                """
                Our parent must always be non-null. We must have already found the right
                node before we reach the parent.
                """);
            current = parent;
        }
    }

    private SyntaxToken DeepestTokenContainingPosition(SyntaxNode parent, int position)
    {
        var current = parent;

        while (true)
        {
            var child = current.ChildThatContainsPosition(position);
            if (child == default)
                return default;

            if (child.IsToken)
                return child.AsToken();

            var node = child.AsNode();
            Debug.Assert(node is not null);
            current = node;
        }
    }

    private SyntaxNode? DeepestNodeContainingPosition(SyntaxNode parent, int position)
    {
        var current = parent;

        while (true)
        {
            var child = current.ChildThatContainsPosition(position);
            if (child == default)
                return current;

            if (child.IsToken)
                return current;

            var node = child.AsNode();
            Debug.Assert(node is not null);
            current = node;
        }
    }

    private sealed class DocumentLineDictionary<T>
    {
        private readonly ConcurrentDictionary<int, T> _dictionary = new();

        public T GetOrAdd(DocumentLine line, Func<int, T> factory)
        {
            return _dictionary.GetOrAdd(line.LineNumber, factory);
        }

        public T GetOrAdd(DocumentLine line, Func<T> factory)
        {
            return _dictionary.GetOrAdd(line.LineNumber, _ => factory());
        }
    }

    public readonly record struct SymbolTypeKind(SymbolKind SymbolKind, TypeKind TypeKind)
    {
        public static readonly SymbolTypeKind TypeParameter
            = new(SymbolKind.TypeParameter, TypeKind.TypeParameter);

        public static implicit operator SymbolTypeKind(SymbolKind kind)
            => new(kind, default);
        public static implicit operator SymbolTypeKind(TypeKind kind)
            => new(SymbolKind.NamedType, kind);
    }
}

partial class CSharpRoslynColorizer
{
    public static class Styles
    {
        // Token kinds
        public static SolidColorBrush CommentForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush DocumentationForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush StringLiteralForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush NumericLiteralForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush KeywordForeground
            = new SolidColorBrush(0xFF4EC9B0);

        // Symbol kinds
        public static SolidColorBrush PropertyForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush FieldForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush EventForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush MethodForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush LocalForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush LabelForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static LinearGradientBrush RangeVariableForeground
            = new LinearGradientBrush
            {
                GradientStops =
                {
                    new GradientStop(Color.FromUInt32(0xFF000000), 0),
                    new GradientStop(Color.FromUInt32(0xFF000000), 1),
                }
            };

        public static LinearGradientBrush PreprocessingForeground
            = new LinearGradientBrush
            {
                GradientStops =
                {
                    new GradientStop(Color.FromUInt32(0xFF000000), 0),
                    new GradientStop(Color.FromUInt32(0xFF000000), 1),
                }
            };

        // Named type kinds
        public static SolidColorBrush TypeParameterForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush ClassForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush StructForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush InterfaceForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush EnumForeground
            = new SolidColorBrush(0xFF4EC9B0);

        public static SolidColorBrush DelegateForeground
            = new SolidColorBrush(0xFF4EC9B0);
    }
}
