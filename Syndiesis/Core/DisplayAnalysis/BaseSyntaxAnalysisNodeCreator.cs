using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public abstract partial class BaseSyntaxAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    protected static readonly InterestingPropertyFilterCache PropertyCache
        = new(SyntaxNodePropertyFilter.Instance);

    public BaseSyntaxAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    { }

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value, DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case SyntaxTree tree:
                return CreateRootTree(tree, valueSource);

            case SyntaxNodeOrToken nodeOrToken:
                return CreateRootNodeOrToken(nodeOrToken, valueSource);

            case SyntaxNode node:
                return CreateRootNode(node, valueSource);

            case SyntaxToken token:
                return CreateRootToken(token, valueSource);

            case ReadOnlySyntaxNodeList nodeList:
                return CreateRootNodeList(nodeList, valueSource);

            case SyntaxTokenList tokenList:
                return CreateRootTokenList(tokenList, valueSource);

            case SyntaxTrivia trivia:
                return CreateRootTrivia(trivia, valueSource);

            case SyntaxTriviaList triviaList:
                return CreateRootTriviaList(triviaList, valueSource);

            case SyntaxReference reference:
                return CreateRootSyntaxReference(reference, valueSource);

            case TextSpan span:
                return CreateRootTextSpan(span, valueSource);

            case SyntaxAnnotation annotation:
                return CreateRootSyntaxAnnotation(annotation, valueSource);
        }

        return null;
    }

    public abstract AnalysisTreeListNode CreateRootTree(
        SyntaxTree tree, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootToken(
        SyntaxToken token, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootNodeList(
        ReadOnlySyntaxNodeList node, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootTokenList(
        SyntaxTokenList list, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootTrivia(
        SyntaxTrivia trivia, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootTriviaList(
        SyntaxTriviaList triviaList, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootSyntaxReference(
        SyntaxReference reference, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootTextSpan(
        TextSpan span, DisplayValueSource valueSource = default);

    public abstract AnalysisTreeListNode CreateRootSyntaxAnnotationList(
        IReadOnlyList<SyntaxAnnotation> annotations,
        ComplexDisplayValueSource valueSource);

    public abstract AnalysisTreeListNode CreateRootSyntaxAnnotation(
        SyntaxAnnotation annotation, DisplayValueSource valueSource);

    public abstract AnalysisTreeListNode CreateChildlessRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default);

    protected IReadOnlyList<AnalysisTreeListNode> CreateNodeListChildren(SyntaxNodeOrTokenList list)
    {
        return list
            .Select(s => CreateRootNodeOrToken(s))
            .ToList()
            ;
    }

    protected static IReadOnlyList<PropertyInfo> GetInterestingPropertiesForNodeType(SyntaxNode node)
    {
        var nodeType = node.GetType();
        var properties = PropertyCache.FilterForType(nodeType).Properties;
        return properties;
    }

    protected void AppendSyntaxTypeDetails(Type type, GroupedRunInlineCollection inlines)
    {
        var typeNameRun = SyntaxTypeDetailsGroupedRun(type);
        inlines.Add(typeNameRun);
    }

    protected GroupedRunInline SyntaxTypeDetailsGroupedRun(Type type)
    {
        if (type.IsGenericType)
        {
            var originalDefinition = type.GetGenericTypeDefinition();
            if (originalDefinition == typeof(SeparatedSyntaxList<>) ||
                originalDefinition == typeof(SyntaxList<>))
            {
                const string genericSuffix = "`1";
                string name = originalDefinition.Name[..^genericSuffix.Length];
                var outerRun = Run($"{name}<", Styles.SyntaxListBrush);
                var closingTag = Run(">", Styles.SyntaxListBrush);
                var argument = type.GenericTypeArguments[0];
                var inner = SyntaxTypeDetailsGroupedRun(argument);

                return new ComplexGroupedRunInline([
                    outerRun,
                    new(inner),
                    closingTag,
                ]);
            }

            throw new UnreachableException("We should have handled any incoming generic type");
        }

        var typeName = type.Name;

        if (typeName is nameof(SyntaxTokenList))
        {
            var run = Run(typeName, Styles.TokenListBrush);
            return new SingleRunInline(run);
        }

        if (typeName is nameof(SyntaxTriviaList))
        {
            var run = Run(typeName, Styles.TriviaListTypeBrush);
            return new SingleRunInline(run);
        }

        const string syntaxSuffix = "Syntax";
        return TypeDisplayWithFadeSuffix(
            typeName, syntaxSuffix, CommonStyles.ClassMainBrush, CommonStyles.ClassSecondaryBrush);
    }
}

partial class BaseSyntaxAnalysisNodeCreator
{
    public static SyntaxStyles Styles
        => AppSettings.Instance.StylePreferences.SyntaxStyles!;

    public abstract class Types : CommonTypes
    {
        public const string CSharpTree = "C#";
        public const string VisualBasicTree = "VB";

        public const string Node = "N";
        public const string SyntaxList = "SL";
        public const string Token = "T";
        public const string TokenList = "TL";
        public const string DisplayValue = "D";
        public const string TriviaList = "VL";

        public const string SyntaxAnnotation = "!";
        public const string SyntaxAnnotationList = "!!";

        // using & to denote reference, there's too many types
        // beginning with S and we want to avoid the confusion
        public const string SyntaxReference = "&";
        public const string TextSpan = "..";

        public const string WhitespaceTrivia = "_";
        public const string CommentTrivia = "/*";
        public const string DirectiveTrivia = "#";
        public const string DisabledTextTrivia = "~";
        public const string EndOfLineTrivia = @"\n";
    }

    [SolidColor("CSharpTree", 0xFF33E5A5)]
    [SolidColor("VisualBasicTree", 0xFF74A3FF)]
    [SolidColor("WhitespaceTrivia", 0xFFB3B3B3)]
    [SolidColor("WhitespaceTriviaKind", 0xFF808080)]
    [SolidColor("TokenKind", 0xFF7A68E5)]
    [SolidColor("FadeTokenKind", 0xFF514599)]
    [SolidColor("TokenList", 0xFF74A3FF)]
    [SolidColor("SyntaxList", 0xFF79BCA4)]
    [SolidColor("Eof", 0xFF76788B)]
    [SolidColor("BasicTriviaNodeType", 0xFF7C7C7C)]
    [SolidColor("WhitespaceTriviaNodeType", 0xFF7C7C7C)]
    [SolidColor("TriviaListType", 0xFF7C7C7C)]
    [SolidColor("DisplayValueNodeType", 0xFFCC935F)]
    [SolidColor("CommentTriviaNodeType", 0xFF00A858)]
    [SolidColor("CommentTriviaContent", 0xFF00703A)]
    [SolidColor("CommentTriviaTokenKind", 0xFF004D28)]
    [SolidColor("DisabledTextTriviaNodeType", 0xFF8B4D4D)]
    [SolidColor("DisabledTextTriviaContent", 0xFF664747)]
    [SolidColor("DisabledTextTriviaTokenKind", 0xFF4D3636)]
    [SolidColor("MissingTokenIndicator", 0xFF8B4D4D)]
    [SolidColor("AnnotationIndicator", 0xFFA86932)]
    public partial class SyntaxStyles
    {
        // Tree displays
        public NodeTypeDisplay CSharpTreeDisplay
            => new(Types.CSharpTree, CSharpTreeColor);

        public NodeTypeDisplay VisualBasicTreeDisplay
            => new(Types.VisualBasicTree, VisualBasicTreeColor);

        // Node displays
        public NodeTypeDisplay ClassNodeDisplay
            => new(Types.Node, CommonStyles.ClassMainColor);

        public NodeTypeDisplay SyntaxListNodeDisplay
            => new(Types.SyntaxList, SyntaxListColor);

        public NodeTypeDisplay TokenListNodeDisplay
            => new(Types.TokenList, TokenListColor);

        public NodeTypeDisplay TokenNodeDisplay
            => new(Types.Token, TokenKindColor);

        public NodeTypeDisplay DisplayValueDisplay
            => new(Types.DisplayValue, DisplayValueNodeTypeColor);

        public NodeTypeDisplay TriviaListDisplay
            => new(Types.TriviaList, BasicTriviaNodeTypeColor);

        public NodeTypeDisplay SyntaxReferenceDisplay
            => new(Types.SyntaxReference, CommonStyles.ClassMainColor);

        public NodeTypeDisplay TextSpanDisplay
            => new(Types.TextSpan, CommonStyles.StructMainColor);

        public NodeTypeDisplay SyntaxAnnotationDisplay
            => new(Types.SyntaxAnnotation, AnnotationIndicatorColor);

        public NodeTypeDisplay SyntaxAnnotationListDisplay
            => new(Types.SyntaxAnnotationList, AnnotationIndicatorColor);

        // Trivia displays
        public NodeTypeDisplay WhitespaceTriviaDisplay
            => new(Types.WhitespaceTrivia, WhitespaceTriviaColor);

        public NodeTypeDisplay DirectiveTriviaDisplay
            => new(Types.DirectiveTrivia, BasicTriviaNodeTypeColor);

        public NodeTypeDisplay EndOfLineTriviaDisplay
            => new(Types.EndOfLineTrivia, BasicTriviaNodeTypeColor);

        public NodeTypeDisplay CommentTriviaDisplay
            => new(Types.CommentTrivia, CommentTriviaNodeTypeColor);

        public NodeTypeDisplay DisabledTextTriviaDisplay
            => new(Types.DisabledTextTrivia, DisabledTextTriviaNodeTypeColor);
    }
}
