﻿using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Syndiesis.Core;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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

        try
        {
            PerformColorization(line, cancellationTokenFactory.CurrentToken);
        }
        catch (Exception ex)
        when (ex is not OperationCanceledException)
        {
            App.Current.ExceptionListener.HandleException(ex, "Colorization failed");
        }
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

        var parent = startNode.CommonParent(endNode);
        AssertNonNullCommonParent(parent);

        var descendantTrivia = parent.DescendantTrivia(descendIntoTrivia: true);

        foreach (var trivia in descendantTrivia)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var colorizer = GetTriviaColorizer(trivia.Kind());
            ColorizeSpan(line, trivia.Span, colorizer);
        }

        var descendantTokens = parent.DescendantTokens(descendIntoTrivia: true);

        foreach (var token in descendantTokens)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            ColorizeTokenInLine(line, token);
        }
    }

    private void ColorizeTokenInLine(DocumentLine line, SyntaxToken token)
    {
        if (token.Kind() is SyntaxKind.IdentifierToken)
        {
            var symbolKind = GetDeclaringSymbolKind(token);
            if (symbolKind.IsEnumField)
            {
                var enumFieldColorizer = ColorizerForBrush(Styles.EnumFieldBrush);
                ColorizeSpan(line, token.Span, enumFieldColorizer);
                return;
            }

            if (symbolKind.SymbolKind is SymbolKind.Field or SymbolKind.Local)
            {
                bool isConst = IsConstDeclaration(token);
                if (isConst)
                {
                    var constColorizer = ColorizerForBrush(Styles.ConstantBrush);
                    ColorizeSpan(line, token.Span, constColorizer);
                    return;
                }
            }

            var colorizer = GetColorizer(symbolKind);
            ColorizeSpan(line, token.Span, colorizer);
        }
        else
        {
            var xmlColorizer = GetXmlTokenColorizer(token);
            if (xmlColorizer is not null)
            {
                ColorizeSpan(line, token.Span, xmlColorizer);
            }
            else
            {
                var colorizer = GetTokenColorizer(token);
                ColorizeSpan(line, token.Span, colorizer);
            }
        }
    }

    private Action<VisualLineElement>? GetXmlTokenColorizer(SyntaxToken token)
    {
        var tokenKind = token.Kind();

        switch (tokenKind)
        {
            case SyntaxKind.XmlEntityLiteralToken:
                return ColorizerForBrush(Styles.XmlEntityLiteralBrush);

            case SyntaxKind.BeginCDataToken:
            case SyntaxKind.EndCDataToken:
                return ColorizerForBrush(Styles.XmlCDataBrush);
        }

        var tokenParent = token.Parent!;

        if (tokenKind is SyntaxKind.XmlTextLiteralToken)
        {
            var tokenParentKind = tokenParent.Kind();
            switch (tokenParentKind)
            {
                case SyntaxKind.XmlString:
                    return ColorizerForBrush(Styles.XmlTextBrush);

                case SyntaxKind.XmlCDataSection:
                    return ColorizerForBrush(Styles.XmlCDataBrush);
            }
        }

        if (tokenKind is SyntaxKind.XmlNameToken)
        {
            var nameParent = tokenParent.Parent;
            var nameParentKind = nameParent?.Kind();
            switch (nameParentKind)
            {
                case SyntaxKind.XmlAttribute:
                    return ColorizerForBrush(Styles.XmlAttributeBrush);

                case SyntaxKind.XmlElementStartTag:
                case SyntaxKind.XmlElementEndTag:
                case null:
                    return ColorizerForBrush(Styles.XmlTagBrush);
            }
        }

        return null;
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

        var parent = startNode.CommonParent(endNode);
        AssertNonNullCommonParent(parent);

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
                    return ColorizerForBrush(Styles.EnumFieldBrush);
                }

                var field = (IFieldSymbol)symbol;
                if (field.IsConst)
                {
                    return ColorizerForBrush(Styles.ConstantBrush);
                }

                break;
            }

            case SymbolKind.Local:
            {
                var local = (ILocalSymbol)symbol;
                if (local.IsConst)
                {
                    return ColorizerForBrush(Styles.ConstantBrush);
                }

                break;
            }
        }

        return GetColorizer(kind);
    }

    private Action<VisualLineElement>? GetTokenColorizer(SyntaxToken token)
    {
        if (token.IsKeyword())
        {
            var parent = token.Parent;
            if (parent is DirectiveTriviaSyntax)
                return null;
        }

        var manualColorizer = GetTokenColorizer(token.Kind());
        return manualColorizer;
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

    private Action<VisualLineElement>? ColorizerForBrush(ILazilyUpdatedBrush? brush)
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

    private static ILazilyUpdatedBrush? BrushForTokenSyntaxKind(SyntaxKind kind)
    {
        if (SyntaxFacts.IsKeywordKind(kind))
            return Styles.KeywordBrush;

        return kind switch
        {
            SyntaxKind.IntegerLiteralToken or
            SyntaxKind.DecimalLiteralToken or
            SyntaxKind.FloatingLiteralToken => Styles.NumericLiteralBrush,

            SyntaxKind.DateLiteralToken or
            SyntaxKind.StringLiteralToken or
            SyntaxKind.InterpolatedStringTextToken or
            SyntaxKind.InterpolatedStringText or
            SyntaxKind.CharacterLiteralToken => Styles.StringLiteralBrush,

            _ => null,
        };
    }

    private static ILazilyUpdatedBrush? BrushForTriviaSyntaxKind(SyntaxKind kind)
    {
        if (SyntaxFacts.IsPreprocessorDirective(kind))
            return Styles.PreprocessingStatementBrush;

        return kind switch
        {
            SyntaxKind.DisabledTextTrivia => Styles.DisabledTextBrush,

            SyntaxKind.CommentTrivia => Styles.CommentBrush,

            SyntaxKind.DocumentationCommentTrivia or
            SyntaxKind.DocumentationCommentExteriorTrivia => Styles.DocumentationBrush,

            SyntaxKind.ConflictMarkerTrivia => Styles.ConflictMarkerBrush,

            _ => null,
        };
    }

    private static ILazilyUpdatedBrush? BrushForSymbolKind(SymbolKind kind)
    {
        return kind switch
        {
            SymbolKind.Field => Styles.FieldBrush,
            SymbolKind.Property => Styles.PropertyBrush,
            SymbolKind.Event => Styles.EventBrush,
            SymbolKind.Method => Styles.MethodBrush,
            SymbolKind.Local => Styles.LocalBrush,
            SymbolKind.Label => Styles.LabelBrush,
            SymbolKind.Parameter => Styles.ParameterBrush,
            SymbolKind.RangeVariable => Styles.RangeVariableBrush,
            SymbolKind.Preprocessing => Styles.PreprocessingBrush,
            SymbolKind.TypeParameter => Styles.TypeParameterBrush,
            SymbolKind.DynamicType => Styles.KeywordBrush,
            _ => null,
        };
    }

    private static ILazilyUpdatedBrush? BrushForTypeKind(TypeKind kind)
    {
        return kind switch
        {
            TypeKind.Module => Styles.ModuleBrush,
            TypeKind.Class => Styles.ClassBrush,
            TypeKind.Struct => Styles.StructBrush,
            TypeKind.Interface => Styles.InterfaceBrush,
            TypeKind.Delegate => Styles.DelegateBrush,
            TypeKind.Enum => Styles.EnumBrush,
            TypeKind.TypeParameter => Styles.TypeParameterBrush,
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

                    case CollectionRangeVariableSyntax:
                    {
                        return SymbolKind.RangeVariable;
                    }

                    case VariableNameEqualsSyntax variableNameEquals:
                    {
                        var container = variableNameEquals.Parent;
                        switch (container)
                        {
                            case AggregationRangeVariableSyntax:
                            case ExpressionRangeVariableSyntax:
                                return SymbolKind.RangeVariable;
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

    private void FormatWith(VisualLineElement element, ILazilyUpdatedBrush brush)
    {
        element.TextRunProperties.SetForegroundBrush(brush.Brush);
    }

    private static void AssertNonNullCommonParent(
        [NotNull]
        SyntaxNode? parent)
    {
        Debug.Assert(
            parent is not null,
            """
            Our parent must always be non-null. We must have already found the right
            node before we reach the parent.
            """);
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
