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

public sealed partial class CSharpRoslynColorizer(CSharpSingleTreeCompilationSource compilationSource)
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

        var startNode = root.DeepestNodeContainingPosition(offset)!;
        var endNode = root.DeepestNodeContainingPosition(endOffset)!;

        var parent = CommonParent(startNode, endNode);

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
            var xmlColorizer = GetXmlTokenColorizer(token);
            if (xmlColorizer is not null)
            {
                ColorizeSpan(line, token.Span, xmlColorizer);
            }

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
            var colorizer = GetTokenColorizer(token);
            ColorizeSpan(line, token.Span, colorizer);
        }
    }

    private Action<VisualLineElement>? GetXmlTokenColorizer(SyntaxToken token)
    {
        var tokenKind = token.Kind();

        switch (tokenKind)
        {
            case SyntaxKind.XmlEntityLiteralToken:
                return ColorizerForBrush(Styles.XmlEntityLiteralBrush);

            case SyntaxKind.XmlCDataStartToken:
            case SyntaxKind.XmlCDataEndToken:
                return ColorizerForBrush(Styles.XmlCDataBrush);
        }

        var tokenParent = token.Parent!;
        var tokenParentKind = tokenParent.Kind();

        if (tokenKind is SyntaxKind.XmlTextLiteralToken)
        {
            switch (tokenParentKind)
            {
                case SyntaxKind.XmlCDataSection:
                    return ColorizerForBrush(Styles.XmlCDataBrush);
            }
        }

        if (tokenParentKind is SyntaxKind.XmlName)
        {
            var nameParent = tokenParent.Parent!;
            var nameParentKind = nameParent.Kind();
            switch (nameParentKind)
            {
                case SyntaxKind.XmlNameAttribute:
                case SyntaxKind.XmlCrefAttribute:
                case SyntaxKind.XmlTextAttribute:
                    return ColorizerForBrush(Styles.XmlAttributeBrush);

                case SyntaxKind.XmlElementStartTag:
                case SyntaxKind.XmlElementEndTag:
                case SyntaxKind.XmlEmptyElement:
                case SyntaxKind.XmlElement:
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

        if (secondIdentifierParent is AliasQualifiedNameSyntax aliasQualified)
        {
            return MatchesAttributeSyntaxTraversal(
                aliasQualified, aliasQualified.Name, identifierParent);
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
        if (parent is QualifiedNameSyntax or AliasQualifiedNameSyntax)
            return false;

        if (parent is AttributeSyntax)
            return true;

        return false;
    }

    private static bool IsPreprocessingIdentifierNode(SyntaxNode node, SemanticModel model)
    {
        bool isDefinition = node.Kind()
            is SyntaxKind.DefineDirectiveTrivia
            or SyntaxKind.UndefDirectiveTrivia
            ;

        if (isDefinition)
            return true;

        // This returns true for `#pragma warning` directives, and is a Roslyn bug
        // https://github.com/dotnet/roslyn/issues/72907
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

            case SymbolKind.Parameter:
            {
                var parameter = (IParameterSymbol)symbol;
                bool isValueOfSetter = parameter
                    is
                    {
                        Name: "value",
                        IsImplicitlyDeclared: true,
                    }
                    && parameter.ContainingSymbol
                    is IMethodSymbol
                    {
                        MethodKind: MethodKind.PropertySet
                            or MethodKind.EventAdd
                            or MethodKind.EventRemove
                    };

                if (isValueOfSetter)
                {
                    return ColorizerForBrush(Styles.KeywordBrush);
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
            SyntaxKind.NumericLiteralToken => Styles.NumericLiteralBrush,

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

            SyntaxKind.SingleLineCommentTrivia or
            SyntaxKind.MultiLineCommentTrivia => Styles.CommentBrush,

            SyntaxKind.DocumentationCommentExteriorTrivia or
            SyntaxKind.SingleLineDocumentationCommentTrivia or
            SyntaxKind.MultiLineDocumentationCommentTrivia => Styles.DocumentationBrush,

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
            _ => null,
        };
    }

    private static ILazilyUpdatedBrush? BrushForTypeKind(TypeKind kind)
    {
        return kind switch
        {
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
                return DeclarationTypeSymbolKind(constructorParent);
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

            case LocalFunctionStatementSyntax localFunctionDeclaration
            when localFunctionDeclaration.Identifier.Span == token.Span:
                return SymbolKind.Method;

            case FromClauseSyntax fromClause
            when fromClause.Identifier.Span == token.Span:
                return SymbolKind.RangeVariable;

            case LetClauseSyntax letClause
            when letClause.Identifier.Span == token.Span:
                return SymbolKind.RangeVariable;

            case QueryContinuationSyntax queryContinuation
            when queryContinuation.Identifier.Span == token.Span:
                return SymbolKind.RangeVariable;
        }

        return default;
    }

    private static SymbolTypeKind DeclarationTypeSymbolKind(
        MemberDeclarationSyntax? declarationSyntax)
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

            case null:
                return default;

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

    private void FormatWith(VisualLineElement element, ILazilyUpdatedBrush brush)
    {
        element.TextRunProperties.SetForegroundBrush(brush.Brush);
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
