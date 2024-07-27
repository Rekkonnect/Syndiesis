using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case SyntaxTree tree:
                return CreateRootTree(tree, valueSource, includeChildren);

            case SyntaxNodeOrToken nodeOrToken:
                return CreateRootNodeOrToken(nodeOrToken, valueSource, includeChildren);

            case SyntaxNode node:
                return CreateRootNode(node, valueSource, includeChildren);

            case SyntaxToken token:
                return CreateRootToken(token, valueSource, includeChildren);

            case ReadOnlySyntaxNodeList nodeList:
                return CreateRootNodeList(nodeList, valueSource, includeChildren);

            case SyntaxTokenList tokenList:
                return CreateRootTokenList(tokenList, valueSource, includeChildren);

            case ChildSyntaxList childList:
                return CreateRootChildSyntaxList(childList, valueSource, includeChildren);

            case SyntaxTrivia trivia:
                return CreateRootTrivia(trivia, valueSource, includeChildren);

            case SyntaxTriviaList triviaList:
                return CreateRootTriviaList(triviaList, valueSource, includeChildren);

            case SyntaxReference reference:
                return CreateRootSyntaxReference(reference, valueSource, includeChildren);

            case TextSpan span:
                return CreateRootTextSpan(span, valueSource, includeChildren);

            case SyntaxAnnotation annotation:
                return CreateRootSyntaxAnnotation(annotation, valueSource, includeChildren);
        }

        return null;
    }

    public abstract AnalysisTreeListNode CreateRootTree<TDisplayValueSource>(
        SyntaxTree tree, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootNodeOrToken<TDisplayValueSource>(
        SyntaxNodeOrToken nodeOrToken, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootNode<TDisplayValueSource>(
        SyntaxNode node, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootToken<TDisplayValueSource>(
        SyntaxToken token, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootNodeList<TDisplayValueSource>(
        ReadOnlySyntaxNodeList node, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootTokenList<TDisplayValueSource>(
        SyntaxTokenList list, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootChildSyntaxList<TDisplayValueSource>(
        ChildSyntaxList list, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootTrivia<TDisplayValueSource>(
        SyntaxTrivia trivia, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootTriviaList<TDisplayValueSource>(
        SyntaxTriviaList triviaList, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootSyntaxReference<TDisplayValueSource>(
        SyntaxReference reference, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootTextSpan<TDisplayValueSource>(
        TextSpan span, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootSyntaxAnnotation<TDisplayValueSource>(
        SyntaxAnnotation annotation, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public abstract AnalysisTreeListNode CreateRootSyntaxAnnotationList<TDisplayValueSource>(
        IReadOnlyList<SyntaxAnnotation> annotations, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
        ;

    protected IReadOnlyList<AnalysisTreeListNode> CreateNodeListChildren(SyntaxNodeOrTokenList list)
    {
        return list
            .Select(s => CreateRootNodeOrToken<IDisplayValueSource>(s, default, true))
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

    public void AppendSyntaxDetails(
        object value, GroupedRunInlineCollection inlines)
    {
        var type = value.GetType();
        AppendSyntaxTypeDetails(type, inlines);
    }

    protected GroupedRunInline SyntaxTypeDetailsGroupedRun(Type type)
    {
        if (type.IsGenericType)
        {
            var originalDefinition = type.GetGenericTypeDefinition();
            var originalName = originalDefinition.Name;
            int firstGenericMarkerIndex = originalName.LastIndexOf('`');
            Debug.Assert(firstGenericMarkerIndex > 0);
            string name = originalName[..firstGenericMarkerIndex];
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
    public abstract class GeneralSyntaxRootViewNodeCreator<TValue, TCreator>(
        TCreator creator)
        : RootViewNodeCreator<TValue, TCreator>(creator)
        where TCreator : BaseSyntaxAnalysisNodeCreator
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.Syntax;
        }

        protected static ComplexDisplayValueSource CreateCommonSyntaxAnnotationsDisplayValue(
            string greenPropertyName)
        {
            return
                new ComplexDisplayValueSource(
                    Property(greenPropertyName),
                    new ComplexDisplayValueSource(
                        MethodSource(RoslynInternalsEx.GetAnnotationsMethodName),
                        null
                    )
                )
                {
                    Modifiers = DisplayValueSource.SymbolKind.Internal,
                };
        }
    }

    public sealed class SyntaxTreeRootViewNodeCreator(BaseSyntaxAnalysisNodeCreator creator)
        : GeneralSyntaxRootViewNodeCreator<SyntaxTree, BaseSyntaxAnalysisNodeCreator>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTree tree, GroupedRunInlineCollection inlines)
        {
            Creator.AppendSyntaxDetails(tree, inlines);

            var language = tree.GetLanguage();

            return AnalysisTreeListNodeLine(
                inlines,
                DisplayForLanguage(language));
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTree tree)
        {
            return () => GetChildren(tree);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            return
            [
                Creator.CreateRootNode(root, MethodSource(nameof(SyntaxTree.GetRoot))),
                Creator.CreateRootGeneral(tree.Options, Property(nameof(SyntaxTree.Options))),
            ];
        }

        private static NodeTypeDisplay DisplayForLanguage(
            [NotNull]
            string? language)
        {
            return language switch
            {
                LanguageNames.CSharp => Styles.CSharpTreeDisplay,
                LanguageNames.VisualBasic => Styles.VisualBasicTreeDisplay,
                _ => throw RoslynExceptions.ThrowInvalidLanguageArgument(language, nameof(language)),
            };
        }
    }
}

partial class BaseSyntaxAnalysisNodeCreator
{
    public static SyntaxStyles Styles
        => AppSettings.Instance.NodeColorPreferences.SyntaxStyles!;

    public abstract class Types : CommonTypes
    {
        public const string CSharpTree = "C#";
        public const string VisualBasicTree = "VB";

        public const string Node = "N";
        public const string SyntaxList = "SL";
        public const string Token = "T";
        public const string TokenList = "TL";
        public const string ChildSyntaxList = "CL";
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
        public const string SplitterTrivia = ":";
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

        public NodeTypeDisplay ChildSyntaxListNodeDisplay
            => new(Types.ChildSyntaxList, SyntaxListColor);

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

        public NodeTypeDisplay SplitterTriviaDisplay
            => new(Types.SplitterTrivia, BasicTriviaNodeTypeColor);
    }
}
