using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Syndiesis.Controls.Editor;

public sealed partial class VisualBasicRoslynColorizer(
    VisualBasicSingleTreeCompilationSource compilationSource)
    : RoslynColorizer(compilationSource)
{
    private readonly DocumentLineDictionary<CancellationTokenFactory> _lineCancellations = new();

    protected override void ColorizeLineEnabled(DocumentLine line)
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
        var tree = source.Tree as VisualBasicSyntaxTree;
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
        DocumentLine line, VisualBasicSyntaxTree tree, CancellationToken cancellationToken)
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

        var startNode = root.DeepestNodeContainingPosition(offset)!;
        var endNode = root.DeepestNodeContainingPosition(endOffset)!;

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

        var startNode = root.DeepestNodeContainingPosition(offset)!;
        var endNode = root.DeepestNodeContainingPosition(endOffset)!;

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

            bool isAttribute = IsAttribute(identifier, symbolInfo);
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

    private bool IsAttribute(SyntaxToken identifier, SymbolInfo symbolInfo)
    {
        if (symbolInfo.Symbol is null)
            return false;

        var identifierParent = identifier.Parent as SimpleNameSyntax;
        if (identifierParent is null)
            return false;

        var secondIdentifierParent = identifierParent.Parent;
        if (secondIdentifierParent is null)
            return false;

        if (secondIdentifierParent is AttributeSyntax)
            return true;

        if (secondIdentifierParent is QualifiedNameSyntax qualified)
        {
            return MatchesAttributeSyntaxTraversal(
                qualified, qualified.Right, identifierParent);
        }

        if (secondIdentifierParent.Parent is AttributeSyntax)
            return true;

        return false;
    }

    private bool MatchesAttributeSyntaxTraversal(
        NameSyntax name,
        NameSyntax right,
        SimpleNameSyntax targetIdentifier)
    {
        // we ensure that the identifier is the rightmost in the qualified name
        var matchesSpan = right.Span == targetIdentifier.Span;
        if (!matchesSpan)
            return false;

        var parent = name.Parent;
        // this is not the rightmost qualified name
        if (parent is QualifiedNameSyntax)
            return false;

        if (parent is AttributeSyntax)
            return true;

        return false;
    }

    private static bool IsPreprocessingIdentifierNode(SyntaxNode node, SemanticModel model)
    {
        bool isDefinition = node.Kind()
            is SyntaxKind.ConstDirectiveTrivia
            ;

        if (isDefinition)
            return true;

        return model.GetPreprocessingSymbolInfo(node).Symbol is not null;
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
                bool withinEnum = symbol.ContainingSymbol
                    is INamedTypeSymbol { TypeKind: TypeKind.Enum };
                if (withinEnum)
                {
                    return ColorizerForBrush(Styles.EnumFieldForeground);
                }

                var field = (IFieldSymbol)symbol;
                if (field.IsConst)
                {
                    return ColorizerForBrush(Styles.ConstantForeground);
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
            SyntaxKind.IntegerLiteralToken or
            SyntaxKind.DecimalLiteralToken or
            SyntaxKind.FloatingLiteralToken => Styles.NumericLiteralForeground,

            SyntaxKind.DateLiteralToken or
            SyntaxKind.StringLiteralToken or
            SyntaxKind.InterpolatedStringTextToken or
            SyntaxKind.InterpolatedStringText or
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
            SyntaxKind.CommentTrivia => Styles.CommentForeground,
            SyntaxKind.DocumentationCommentTrivia or
            SyntaxKind.DocumentationCommentExteriorTrivia => Styles.DocumentationForeground,
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
            TypeKind.Module => Styles.ClassForeground,
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
            case ModifiedIdentifierSyntax modifiedIdentifier:
            {
                var declarator = modifiedIdentifier.Parent as VariableDeclaratorSyntax;
                var container = declarator!.Parent;
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
            case ModuleStatementSyntax moduleDeclaration
            when moduleDeclaration.Identifier.Span == token.Span:
                return TypeKind.Module;
            case ClassStatementSyntax classDeclaration
            when classDeclaration.Identifier.Span == token.Span:
                return TypeKind.Class;
            case StructureStatementSyntax structDeclaration
            when structDeclaration.Identifier.Span == token.Span:
                return TypeKind.Struct;
            case InterfaceStatementSyntax interfaceDeclaration
            when interfaceDeclaration.Identifier.Span == token.Span:
                return TypeKind.Interface;
            case DelegateStatementSyntax delegateDeclaration
            when delegateDeclaration.Identifier.Span == token.Span:
                return TypeKind.Delegate;
            case EnumStatementSyntax enumDeclaration
            when enumDeclaration.Identifier.Span == token.Span:
                return TypeKind.Enum;

            case TypeParameterSyntax typeParameter
            when typeParameter.Identifier.Span == token.Span:
                return SymbolTypeKind.TypeParameter;

            case ParameterSyntax parameterSyntax
            when parameterSyntax.Identifier.Span == token.Span:
                return SymbolKind.Parameter;

            case EnumMemberDeclarationSyntax enumMemberDeclaration
            when enumMemberDeclaration.Identifier.Span == token.Span:
                return SymbolTypeKind.EnumField;

            case MethodStatementSyntax methodDeclaration
            when methodDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Method;

            case PropertyStatementSyntax propertyDeclaration
            when propertyDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Property;

            case EventStatementSyntax eventDeclaration
            when eventDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Event;

            case ModifiedIdentifierSyntax modifiedIdentifier
            when modifiedIdentifier.Identifier.Span == token.Span:
                var identifierParent = modifiedIdentifier.Parent;
                switch (identifierParent)
                {
                    case ParameterSyntax:
                        return SymbolKind.Parameter;
                    case VariableDeclaratorSyntax variableDeclarator:
                    {
                        var container = variableDeclarator.Parent;
                        switch (container)
                        {
                            case LocalDeclarationStatementSyntax:
                                return SymbolKind.Local;
                            case FieldDeclarationSyntax:
                                return SymbolKind.Field;
                            default:
                                return SymbolKind.Alias;
                        }
                    }

                    default:
                        return SymbolKind.Alias;
                }
        }

        return default;
    }

    private static SymbolTypeKind DeclarationTypeSymbolKind(DeclarationStatementSyntax StatementSyntax)
    {
        switch (StatementSyntax)
        {
            case ClassStatementSyntax:
                return TypeKind.Class;
            case StructureStatementSyntax:
                return TypeKind.Struct;
            case InterfaceStatementSyntax:
                return TypeKind.Interface;
            case EnumStatementSyntax:
                return TypeKind.Enum;
            case DelegateStatementSyntax:
                return TypeKind.Delegate;

            default:
                throw new UnreachableException();
        }
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
}

partial class VisualBasicRoslynColorizer
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
