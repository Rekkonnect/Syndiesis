using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Syndiesis.Controls.Editor;

public sealed partial class CSharpRoslynColorizer(SingleTreeCompilationSource compilationSource)
    : RoslynColorizer(compilationSource)
{
    private readonly DocumentLineDictionary<CancellationTokenFactory> _lineCancellations = new();

    public bool Enabled = true;

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!Enabled)
            return;

        if (!AppSettings.Instance.EnableColorization)
            return;

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

        _lineCancellations.Remove(line);
    }

    private void InitiateSyntaxColorization(
        DocumentLine line, CSharpSyntaxTree tree, CancellationToken cancellationToken)
    {
        int offset = line.Offset;
        int endOffset = line.EndOffset;

        if (offset >= tree.Length)
            return;

        if (endOffset >= tree.Length)
        {
            endOffset = tree.Length - 1;
        }

        var root = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        var startNode = DeepestNodeContainingPosition(root, offset)!;
        var endNode = DeepestNodeContainingPosition(root, endOffset)!;

        var parent = CommonParent(startNode, endNode);

        var descendantTokens = parent.DescendantTokens(descendIntoTrivia: true);

        foreach (var token in descendantTokens)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            ColorizeTokenInLine(line, token);
        }

        var descendantTrivia = parent.DescendantTrivia(descendIntoTrivia: true);

        foreach (var trivia in descendantTrivia)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var colorizer = GetTriviaColorizer(trivia.Kind());
            ColorizeSpan(line, trivia.Span, colorizer);
        }

        var descendantNodes = parent.DescendantNodes();

        foreach (var node in descendantNodes)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var colorizer = GetNodeColorizer(node.Kind());
            ColorizeSpan(line, node.Span, colorizer);
        }
    }

    private void ColorizeTokenInLine(DocumentLine line, SyntaxToken token)
    {
        if (token.Kind() is SyntaxKind.IdentifierToken)
        {
            var symbolKind = GetDeclaringSymbolKind(token);
            if (symbolKind.IsEnumField)
            {
                var enumFieldColorizer = ColorizerForBrush(Styles.EnumFieldForeground);
                ColorizeSpan(line, token.Span, enumFieldColorizer);
                return;
            }

            if (symbolKind.SymbolKind is SymbolKind.Field or SymbolKind.Local)
            {
                bool isConst = IsConstDeclaration(token);
                if (isConst)
                {
                    var constColorizer = ColorizerForBrush(Styles.ConstantForeground);
                    ColorizeSpan(line, token.Span, constColorizer);
                    return;
                }
            }

            var colorizer = GetColorizer(symbolKind);
            ColorizeSpan(line, token.Span, colorizer);
        }
        else
        {
            var colorizer = GetTokenColorizer(token.Kind());
            ColorizeSpan(line, token.Span, colorizer);
        }
    }

    private void ColorizeSpan(
        DocumentLine line,
        TextSpan span,
        Action<VisualLineElement>? colorizer)
    {
        if (colorizer is null)
            return;

        int start = Math.Max(line.Offset, span.Start);
        int end = Math.Min(line.EndOffset, span.End);
        if (start >= end)
            return;

        ChangeLinePart(start, end, colorizer);
    }

    private void InitiateSemanticColorization(
        DocumentLine line, SemanticModel model, CancellationToken cancellationToken)
    {
        if (!AppSettings.Instance.EnableSemanticColorization)
            return;

        var tree = model.SyntaxTree;
        int offset = line.Offset;
        int endOffset = line.EndOffset;

        if (offset >= tree.Length)
            return;

        if (endOffset >= tree.Length)
        {
            endOffset = tree.Length - 1;
        }

        var root = tree.GetRoot(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        var startNode = DeepestNodeContainingPosition(root, offset)!;
        var endNode = DeepestNodeContainingPosition(root, endOffset)!;

        var parent = CommonParent(startNode, endNode);

        var lineSpan = TextSpan.FromBounds(offset, endOffset);
        var descendantIdentifiers = parent.DescendantTokens(lineSpan, descendIntoTrivia: true);

        foreach (var identifier in descendantIdentifiers)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (identifier.Kind() is not SyntaxKind.IdentifierToken)
                continue;

            var identifierParent = identifier.Parent!;
            var symbolInfo = model.GetSymbolInfo(identifierParent, cancellationToken);

            bool isVar = IsVar(identifier, symbolInfo);
            if (isVar)
            {
                var varColorizer = GetTokenColorizer(SyntaxKind.VarKeyword);
                ColorizeSpan(line, identifier.Span, varColorizer);
                continue;
            }

            bool isNameOf = IsNameOf(identifier, symbolInfo, model);
            if (isNameOf)
            {
                var nameofColorizer = GetTokenColorizer(SyntaxKind.NameOfKeyword);
                ColorizeSpan(line, identifier.Span, nameofColorizer);
                continue;
            }

            bool isAttribute = IsAttribute(identifier, symbolInfo, model);
            if (isAttribute)
            {
                var attributeColorizer = GetColorizer(TypeKind.Class);
                ColorizeSpan(line, identifier.Span, attributeColorizer);
                continue;
            }

            var isPreprocessing = IsPreprocessingIdentifierNode(identifierParent, model);
            if (isPreprocessing)
            {
                var preprocessingColorizer = GetColorizer(SymbolKind.Preprocessing);
                ColorizeSpan(line, identifier.Span, preprocessingColorizer);
                continue;
            }

            var colorizer = GetColorizer(symbolInfo);
            ColorizeSpan(line, identifier.Span, colorizer);
        }
    }

    private bool IsAttribute(SyntaxToken identifier, SymbolInfo symbolInfo, SemanticModel model)
    {
        var identifierParent = identifier.Parent!;
        if (identifierParent.Parent is not AttributeSyntax attribute)
            return false;

        var typeInfo = model.GetTypeInfo(identifierParent);
        return typeInfo.Type is not null;
    }

    private static bool IsPreprocessingIdentifierNode(SyntaxNode node, SemanticModel model)
    {
        bool isDefinition = node.Kind()
            is SyntaxKind.DefineDirectiveTrivia
            or SyntaxKind.UndefDirectiveTrivia
            ;

        if (isDefinition)
            return true;

        return model.GetPreprocessingSymbolInfo(node).Symbol is not null;
    }

    private bool IsNameOf(
        SyntaxToken token,
        SymbolInfo symbolInfo,
        SemanticModel semanticModel)
    {
        bool basic = token.Kind() is SyntaxKind.IdentifierToken
            && token.Text is "nameof"
            && symbolInfo.Symbol is not IMethodSymbol { Name: "nameof" };

        if (!basic)
            return false;

        var doubleParent = token.Parent!.Parent;
        if (doubleParent is null)
            return false;

        var operation = semanticModel.GetOperation(doubleParent);
        return operation is INameOfOperation;
    }

    private bool IsVar(SyntaxToken token, SymbolInfo symbolInfo)
    {
        return token.Kind() is SyntaxKind.IdentifierToken
            && token.Text is "var"
            && symbolInfo.Symbol is not INamedTypeSymbol { Name: "var" };
    }

    private Action<VisualLineElement>? GetColorizer(SymbolInfo symbolInfo)
    {
        var symbol = symbolInfo.Symbol;
        if (symbol is null)
            return null;

        var kind = symbol.Kind;
        switch (kind)
        {
            case SymbolKind.NamedType:
                return GetColorizer(((INamedTypeSymbol)symbol).TypeKind);

            case SymbolKind.Field:
            {
                var field = (IFieldSymbol)symbol;
                if (field.IsConst)
                {
                    return ColorizerForBrush(Styles.ConstantForeground);
                }

                bool withinEnum = symbol.ContainingSymbol
                    is INamedTypeSymbol { TypeKind: TypeKind.Enum };
                if (withinEnum)
                {
                    return ColorizerForBrush(Styles.EnumFieldForeground);
                }

                break;
            }

            case SymbolKind.Local:
            {
                var local = (ILocalSymbol)symbol;
                if (local.IsConst)
                {
                    return ColorizerForBrush(Styles.ConstantForeground);
                }

                break;
            }
        }

        return GetColorizer(kind);
    }

    private Action<VisualLineElement>? GetTokenColorizer(SyntaxKind kind)
    {
        var brush = BrushForTokenSyntaxKind(kind);
        return ColorizerForBrush(brush);
    }

    private Action<VisualLineElement>? GetTriviaColorizer(SyntaxKind kind)
    {
        var brush = BrushForTriviaSyntaxKind(kind);
        return ColorizerForBrush(brush);
    }

    private Action<VisualLineElement>? GetNodeColorizer(SyntaxKind kind)
    {
        var brush = BrushForNodeSyntaxKind(kind);
        return ColorizerForBrush(brush);
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

    private static IBrush? BrushForTokenSyntaxKind(SyntaxKind kind)
    {
        if (SyntaxFacts.IsKeywordKind(kind))
            return Styles.KeywordForeground;

        return kind switch
        {
            SyntaxKind.NumericLiteralToken => Styles.NumericLiteralForeground,

            SyntaxKind.StringLiteralToken or
            SyntaxKind.InterpolatedSingleLineRawStringStartToken or
            SyntaxKind.InterpolatedMultiLineRawStringStartToken or
            SyntaxKind.InterpolatedRawStringEndToken or
            SyntaxKind.InterpolatedStringStartToken or
            SyntaxKind.InterpolatedStringEndToken or
            SyntaxKind.InterpolatedVerbatimStringStartToken or
            SyntaxKind.InterpolatedStringTextToken or
            SyntaxKind.InterpolatedStringText or
            SyntaxKind.MultiLineRawStringLiteralToken or
            SyntaxKind.SingleLineRawStringLiteralToken or
            SyntaxKind.Utf8StringLiteralToken or
            SyntaxKind.Utf8SingleLineRawStringLiteralToken or
            SyntaxKind.Utf8MultiLineRawStringLiteralToken or
            SyntaxKind.CharacterLiteralToken => Styles.StringLiteralForeground,

            _ => null,
        };
    }

    private static IBrush? BrushForTriviaSyntaxKind(SyntaxKind kind)
    {
        if (SyntaxFacts.IsPreprocessorDirective(kind))
            return Styles.PreprocessingStatementForeground;

        return kind switch
        {
            SyntaxKind.DisabledTextTrivia => Styles.DisabledTextForeground,

            SyntaxKind.SingleLineCommentTrivia or
            SyntaxKind.MultiLineCommentTrivia => Styles.CommentForeground,

            SyntaxKind.SingleLineDocumentationCommentTrivia or
            SyntaxKind.MultiLineDocumentationCommentTrivia => Styles.DocumentationForeground,

            _ => null,
        };
    }

    private static IBrush? BrushForNodeSyntaxKind(SyntaxKind kind)
    {
        return kind switch
        {
            _ => null,
        };
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
            SymbolKind.Parameter => Styles.ParameterForeground,
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
            TypeKind.Class => Styles.ClassForeground,
            TypeKind.Struct => Styles.StructForeground,
            TypeKind.Interface => Styles.InterfaceForeground,
            TypeKind.Delegate => Styles.DelegateForeground,
            TypeKind.Enum => Styles.EnumForeground,
            TypeKind.TypeParameter => Styles.TypeParameterForeground,
            _ => null,
        };
    }

    private static bool IsConstDeclaration(SyntaxToken token)
    {
        var parent = token.Parent!;
        switch (parent)
        {
            case VariableDeclaratorSyntax variableDeclarator:
            {
                var declaration = variableDeclarator.Parent as VariableDeclarationSyntax;
                var container = declaration!.Parent;
                return container switch
                {
                    FieldDeclarationSyntax fieldDeclaration =>
                        fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword),
                    LocalDeclarationStatementSyntax localDeclaration =>
                        localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword),
                    _ => false,
                };
            }

            default:
                return false;
        }
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
                return RecordDeclarationTypeKind(recordDeclaration);

            case TypeParameterSyntax typeParameter
            when typeParameter.Identifier.Span == token.Span:
                return SymbolTypeKind.TypeParameter;

            case ParameterSyntax parameterSyntax
            when parameterSyntax.Identifier.Span == token.Span:
                return SymbolKind.Parameter;

            case EnumMemberDeclarationSyntax enumMemberDeclaration
            when enumMemberDeclaration.Identifier.Span == token.Span:
                return SymbolTypeKind.EnumField;

            case VariableDeclaratorSyntax variableDeclarator
            when variableDeclarator.Identifier.Span == token.Span:
            {
                var declaration = variableDeclarator.Parent as VariableDeclarationSyntax;
                var container = declaration!.Parent;
                return container switch
                {
                    FieldDeclarationSyntax => SymbolKind.Field,
                    EventFieldDeclarationSyntax => SymbolKind.Event,
                    _ => SymbolKind.Local,
                };
            }

            case SingleVariableDesignationSyntax variableDesignation
            when variableDesignation.Identifier.Span == token.Span:
                return SymbolKind.Local;

            case ForEachStatementSyntax forEachStatement
            when forEachStatement.Identifier.Span == token.Span:
                return SymbolKind.Local;

            case ConstructorDeclarationSyntax constructorDeclaration
            when constructorDeclaration.Identifier.Span == token.Span:
            {
                var constructorParent = constructorDeclaration.Parent as MemberDeclarationSyntax;
                return DeclarationTypeSymbolKind(constructorParent!);
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

    private static SymbolTypeKind DeclarationTypeSymbolKind(MemberDeclarationSyntax declarationSyntax)
    {
        switch (declarationSyntax)
        {
            case ClassDeclarationSyntax:
                return TypeKind.Class;
            case StructDeclarationSyntax:
                return TypeKind.Struct;
            case InterfaceDeclarationSyntax:
                return TypeKind.Interface;
            case EnumDeclarationSyntax:
                return TypeKind.Enum;
            case DelegateDeclarationSyntax:
                return TypeKind.Delegate;
            case RecordDeclarationSyntax recordDeclaration:
                return RecordDeclarationTypeKind(recordDeclaration);

            default:
                throw new UnreachableException();
        }
    }

    private static TypeKind RecordDeclarationTypeKind(RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.ClassOrStructKeyword.Kind() switch
        {
            SyntaxKind.None or
            SyntaxKind.ClassConstraint => TypeKind.Class,
            SyntaxKind.StructKeyword => TypeKind.Struct,
            _ => throw new UnreachableException(),
        };
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

        public void Remove(DocumentLine line)
        {
            _dictionary.TryRemove(line.LineNumber, out _);
        }
    }

    public readonly record struct SymbolTypeKind(SymbolKind SymbolKind, TypeKind TypeKind)
    {
        public static readonly SymbolTypeKind TypeParameter
            = new(SymbolKind.TypeParameter, TypeKind.TypeParameter);

        public static readonly SymbolTypeKind EnumField
            = new SymbolTypeKind() with
            {
                IsEnumField = true
            };

        public bool IsEnumField { get; init; }

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
        public static readonly SolidColorBrush CommentForeground
            = new(0xFF57A64A);

        public static readonly SolidColorBrush DocumentationForeground
            = new(0xFF608B4E);

        public static readonly SolidColorBrush StringLiteralForeground
            = new(0xFFD69D85);

        public static readonly SolidColorBrush PreprocessingStatementForeground
            = new(0xFF9B9B9B);

        public static readonly SolidColorBrush DisabledTextForeground
            = new(0xFF767676);

        public static readonly SolidColorBrush NumericLiteralForeground
            = new(0xFFBB5E00);

        public static readonly SolidColorBrush KeywordForeground
            = new(0xFF569CD6);

        // Symbol kinds
        public static readonly SolidColorBrush PropertyForeground
            = new(0xFFFFC9B9);

        public static readonly SolidColorBrush FieldForeground
            = new(0xFFDCDCDC);

        public static readonly SolidColorBrush EventForeground
            = new(0xFFFFB9EA);

        public static readonly SolidColorBrush MethodForeground
            = new(0xFFFFF4B9);

        public static readonly SolidColorBrush LocalForeground
            = new(0xFF88EAFF);

        public static readonly SolidColorBrush ParameterForeground
            = new(0xFF88EAFF);

        public static readonly SolidColorBrush LabelForeground
            = new(0xFFDCDCDC);

        public static readonly SolidColorBrush ConstantForeground
            = new(0xFFC0B9FF);

        public static readonly SolidColorBrush EnumFieldForeground
            = new(0xFFE9A0FA);

        // Give a premium feeling
        public static readonly LinearGradientBrush RangeVariableForeground
            = new()
            {
                StartPoint = new(new(0, 0), RelativeUnit.Relative),
                EndPoint = new(new(0.2, 1), RelativeUnit.Relative),
                SpreadMethod = GradientSpreadMethod.Reflect,
                GradientStops =
                {
                    new(Color.FromUInt32(0xFFDCDCDC), 0),
                    new(Color.FromUInt32(0xFF88EAFF), 1),
                }
            };

        public static readonly LinearGradientBrush PreprocessingForeground
            = new()
            {
                StartPoint = new(new(0, 0), RelativeUnit.Relative),
                EndPoint = new(new(0.2, 1), RelativeUnit.Relative),
                SpreadMethod = GradientSpreadMethod.Reflect,
                GradientStops =
                {
                    new(Color.FromUInt32(0xFF9B9B9B), 0),
                    new(Color.FromUInt32(0xFFCBCBCB), 1),
                }
            };

        public static readonly LinearGradientBrush ConflictMarkerForeground
            = new()
            {
                StartPoint = new(new(0, 0), RelativeUnit.Relative),
                EndPoint = new(new(0.2, 1), RelativeUnit.Relative),
                SpreadMethod = GradientSpreadMethod.Reflect,
                GradientStops =
                {
                    new(Color.FromUInt32(0xFFA699E6), 0),
                    new(Color.FromUInt32(0xFFFF01C1), 1),
                }
            };

        // Named type kinds
        public static readonly SolidColorBrush TypeParameterForeground
            = new(0xFFBFD39A);

        public static readonly SolidColorBrush ClassForeground
            = new(0xFF4EC9B0);

        public static readonly SolidColorBrush StructForeground
            = new(0xFF4DCA85);

        public static readonly SolidColorBrush InterfaceForeground
            = new(0xFFA2D080);

        public static readonly SolidColorBrush EnumForeground
            = new(0xFFB8D7A3);

        public static readonly SolidColorBrush DelegateForeground
            = new(0xFF4BCBC8);
    }
}
