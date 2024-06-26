﻿using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed partial class VisualBasicSyntaxAnalysisNodeCreator : BaseSyntaxAnalysisNodeCreator
{
    // node creators
    private readonly SyntaxTreeRootViewNodeCreator _treeCreator;
    private readonly NodeOrTokenRootViewNodeCreator _nodeOrTokenCreator;
    private readonly SyntaxNodeRootViewNodeCreator _syntaxNodeCreator;
    private readonly SyntaxTokenRootViewNodeCreator _syntaxTokenCreator;
    private readonly SyntaxNodeListRootViewNodeCreator _syntaxNodeListCreator;
    private readonly SyntaxTokenListRootViewNodeCreator _syntaxTokenListCreator;
    private readonly SyntaxTriviaRootViewNodeCreator _syntaxTriviaCreator;
    private readonly SyntaxTriviaListRootViewNodeCreator _syntaxTriviaListCreator;
    private readonly SyntaxReferenceRootViewNodeCreator _syntaxReferenceCreator;
    private readonly TextSpanRootViewNodeCreator _textSpanCreator;
    private readonly SyntaxAnnotationRootViewNodeCreator _syntaxAnnotationCreator;
    private readonly SyntaxAnnotationListRootViewNodeCreator _syntaxAnnotationListCreator;

    public VisualBasicSyntaxAnalysisNodeCreator(
        VisualBasicAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _treeCreator = new(this);
        _nodeOrTokenCreator = new(this);
        _syntaxNodeCreator = new(this);
        _syntaxTokenCreator = new(this);
        _syntaxNodeListCreator = new(this);
        _syntaxTokenListCreator = new(this);
        _syntaxTriviaCreator = new(this);
        _syntaxTriviaListCreator = new(this);
        _syntaxReferenceCreator = new(this);
        _textSpanCreator = new(this);
        _syntaxAnnotationCreator = new(this);
        _syntaxAnnotationListCreator = new(this);
    }

    public override AnalysisTreeListNode CreateRootTree(
        SyntaxTree tree, DisplayValueSource valueSource = default)
    {
        return _treeCreator.CreateNode(tree, valueSource);
    }

    public override AnalysisTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, DisplayValueSource valueSource = default)
    {
        return _nodeOrTokenCreator.CreateNode(nodeOrToken, valueSource);
    }

    public override AnalysisTreeListNode CreateRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default)
    {
        return _syntaxNodeCreator.CreateNode(node, valueSource);
    }

    public override AnalysisTreeListNode CreateRootToken(
        SyntaxToken token, DisplayValueSource valueSource = default)
    {
        return _syntaxTokenCreator.CreateNode(token, valueSource);
    }

    public override AnalysisTreeListNode CreateRootNodeList(
        ReadOnlySyntaxNodeList node, DisplayValueSource valueSource = default)
    {
        return _syntaxNodeListCreator.CreateNode(node, valueSource);
    }

    public override AnalysisTreeListNode CreateRootTokenList(
        SyntaxTokenList list, DisplayValueSource valueSource)
    {
        return _syntaxTokenListCreator.CreateNode(list, valueSource);
    }

    public override AnalysisTreeListNode CreateRootTrivia(
        SyntaxTrivia trivia, DisplayValueSource valueSource = default)
    {
        return _syntaxTriviaCreator.CreateNode(trivia, valueSource);
    }

    public override AnalysisTreeListNode CreateRootTriviaList(
        SyntaxTriviaList triviaList, DisplayValueSource valueSource)
    {
        return _syntaxTriviaListCreator.CreateNode(triviaList, valueSource);
    }

    public override AnalysisTreeListNode CreateRootSyntaxReference(
        SyntaxReference reference, DisplayValueSource valueSource)
    {
        return _syntaxReferenceCreator.CreateNode(reference, valueSource);
    }

    public override AnalysisTreeListNode CreateRootTextSpan(
        TextSpan span, DisplayValueSource valueSource)
    {
        return _textSpanCreator.CreateNode(span, valueSource);
    }

    public override AnalysisTreeListNode CreateRootSyntaxAnnotationList(
        IReadOnlyList<SyntaxAnnotation> annotations,
        ComplexDisplayValueSource valueSource)
    {
        return _syntaxAnnotationListCreator.CreateNode(annotations, valueSource);
    }

    public override AnalysisTreeListNode CreateRootSyntaxAnnotation(
        SyntaxAnnotation annotation, DisplayValueSource valueSource)
    {
        return _syntaxAnnotationCreator.CreateNode(annotation, valueSource);
    }

    public override AnalysisTreeListNode CreateChildlessRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default)
    {
        return _syntaxNodeCreator.CreateChildlessNode(node, valueSource);
    }
}

partial class VisualBasicSyntaxAnalysisNodeCreator
{
    public abstract class SyntaxRootViewNodeCreator<TValue>(VisualBasicSyntaxAnalysisNodeCreator creator)
        : GeneralSyntaxRootViewNodeCreator<TValue, VisualBasicSyntaxAnalysisNodeCreator>(creator)
    {
    }

    public sealed class NodeOrTokenRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxNodeOrToken>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxNodeOrToken value, DisplayValueSource valueSource)
        {
            if (value.IsNode)
            {
                return Creator._syntaxNodeCreator.CreateNodeLine(value.AsNode()!, valueSource);
            }

            return Creator._syntaxTokenCreator.CreateNodeLine(value.AsToken(), valueSource);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxNodeOrToken value)
        {
            if (value.IsNode)
            {
                return Creator._syntaxNodeCreator.GetChildRetriever(value.AsNode()!);
            }

            return Creator._syntaxTokenCreator.GetChildRetriever(value.AsToken());
        }
    }

    public class SyntaxAnnotationListRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<IReadOnlyList<SyntaxAnnotation>>(creator)
    {
        public AnalysisTreeListNode CreateNode(
            IReadOnlyList<SyntaxAnnotation> value,
            ComplexDisplayValueSource valueSource)
        {
            var rootLine = CreateNodeLine(value, valueSource);
            var children = GetChildRetriever(value);
            rootLine.AnalysisNodeKind = GetNodeKind(value);
            return AnalysisTreeListNode(
                rootLine,
                children,
                null
            );
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<SyntaxAnnotation> annotations,
            DisplayValueSource valueSource)
        {
            throw new NotSupportedException(
                $"Do not invoke this method; instead use the overload with {nameof(ComplexDisplayValueSource)}");
        }

        public AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<SyntaxAnnotation> annotations,
            ComplexDisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendComplexValueSource(valueSource, inlines);
            var typeDisplay = TypeDisplayGroupedRun(annotations.GetType());
            inlines.Add(typeDisplay);
            inlines.Add(NewValueKindSplitterRun());
            var countDisplayRun = CountDisplayRunGroup(annotations)!;
            inlines.Add(countDisplayRun);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SyntaxAnnotationListDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            IReadOnlyList<SyntaxAnnotation> annotations)
        {
            if (annotations.Count is 0)
                return null;

            Log.Information($"We found syntax annotations: {annotations.Count}");
            return () => CreateNodeChildren(annotations);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateNodeChildren(
            IReadOnlyList<SyntaxAnnotation> annotations)
        {
            return annotations
                .Select(s => Creator.CreateRootSyntaxAnnotation(s, default))
                .ToList()
                ;
        }
    }

    public class SyntaxAnnotationRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxAnnotation>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxAnnotation annotation, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            var run = TypeDisplayGroupedRun(typeof(SyntaxAnnotation));
            inlines.Add(run);

            if (annotation.IsElastic())
            {
                inlines.Add(NewValueKindSplitterRun());
                inlines.Add(CreateElasticAnnotationRun());
            }

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SyntaxAnnotationDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxAnnotation annotation)
        {
            return () => CreateNodeChildren(annotation);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateNodeChildren(SyntaxAnnotation annotation)
        {
            return
            [
                Creator.CreateRootBasic(
                    annotation.Kind,
                    Property(nameof(SyntaxAnnotation.Kind))),

                Creator.CreateRootBasic(
                    annotation.Data,
                    Property(nameof(SyntaxAnnotation.Data))),
            ];
        }

        private static SingleRunInline CreateElasticAnnotationRun()
        {
            return new(Run(
                nameof(SyntaxAnnotation.ElasticAnnotation),
                CommonStyles.PropertyBrush,
                FontStyle.Italic));
        }
    }

    public class SyntaxNodeRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxNode>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxNode node, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateSyntaxTypeInlines(node, valueSource);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.ClassNodeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxNode node)
        {
            if (node.IsMissing && !node.HasAnyTrivia())
                return null;

            return () => CreateNodeChildren(node);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateNodeChildren(SyntaxNode node)
        {
            var properties = GetInterestingPropertiesForNodeType(node);

            var children = new List<AnalysisTreeListNode>(properties.Count);

            foreach (var property in properties)
            {
                var value = property.GetValue(node);
                bool includeValue = ShouldIncludeValue(value);
                if (!includeValue)
                {
                    continue;
                }

                var valueSource = Property(property.Name);

                var childNode = Creator.CreateRootViewNode(value, valueSource);
                if (childNode is not null)
                {
                    children.Add(childNode);
                }
            }

            children.Sort(AnalysisTreeViewNodeBuilderObjectSpanComparer.Instance);

            if (Options.ShowSyntaxAnnotations)
            {
                children.Insert(0, CreateGreenNodeAnnotationsNode(node));
            }

            return children;
        }

        private bool ShouldIncludeValue(object? value)
        {
            switch (value)
            {
                case null:
                    return false;

                case ReadOnlySyntaxNodeList nodeList:
                    return nodeList.Count > 0;

                case SyntaxNode:
                    return true;

                case SyntaxTokenList tokenList:
                    if (tokenList == default)
                        return false;

                    return tokenList.Count > 0;

                case SyntaxTriviaList triviaList:
                    if (!Options.ShowTrivia)
                        return false;

                    if (triviaList == default)
                        return false;

                    return triviaList.Count > 0;

                case SyntaxToken token:
                    if (token.IsMissing)
                    {
                        return true;
                    }

                    return token != default;

                case SyntaxTrivia trivia:
                    if (!Options.ShowTrivia)
                        return false;

                    return trivia != default;

                default:
                    return false;
            }
        }

        private AnalysisTreeListNode CreateGreenNodeAnnotationsNode(SyntaxNode node)
        {
            var valueSource = CreateCommonSyntaxAnnotationsDisplayValue("Green");
            var annotations = RoslynInternalsEx.GetSyntaxAnnotations(node);
            return Creator.CreateRootSyntaxAnnotationList(annotations, valueSource);
        }
    }

    public sealed class SyntaxTokenRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxToken>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxToken token, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            AppendValueSource(valueSource, inlines);
            AppendTokenKindDetails(token, valueSource.Name, inlines);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TokenNodeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.DocumentationCommentLineBreakToken:
                    return null;
            }

            if (token.IsFullEmpty())
                return null;

            return () => CreateTokenChildren(token);
        }

        private void AppendTokenKindDetails(
            SyntaxToken token, string? propertyName, GroupedRunInlineCollection inlines)
        {
            var kind = token.Kind();
            var kindName = kind.ToString();
            bool hasEqualName = propertyName == kindName;
            bool isKeyword = SyntaxFacts.IsKeywordKind(kind);
            var displayTextRun = CreateDisplayTextRun(token, kind, isKeyword);

            bool needsFadeBrush = hasEqualName || isKeyword;
            var kindBrush = needsFadeBrush
                ? Styles.FadeTokenKindBrush
                : Styles.TokenKindBrush;
            var kindRun = Run(kindName, kindBrush, FontStyle.Italic);

            inlines.Add(displayTextRun);
            inlines.Add(NewValueKindSplitterRun());
            inlines.AddSingle(kindRun);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateTokenChildren(SyntaxToken token)
        {
            SyntaxTriviaList leadingTrivia = default;
            SyntaxTriviaList trailingTrivia = default;
            if (Options.ShowTrivia)
            {
                leadingTrivia = token.LeadingTrivia;
                trailingTrivia = token.TrailingTrivia;
            }

            var children = new List<AnalysisTreeListNode>(3);

            // they will be sorted anyway
            AppendTriviaListNode(
                leadingTrivia,
                children,
                Property(nameof(SyntaxToken.LeadingTrivia)));
            AppendTriviaListNode(
                trailingTrivia,
                children,
                Property(nameof(SyntaxToken.TrailingTrivia)));

            switch (token.Kind())
            {
                case SyntaxKind.EndOfFileToken:
                {
                    var displayNode = CreateEndOfFileDisplayNode(token);
                    children.Add(displayNode);
                    break;
                }
                default:
                {
                    if (token.Text.Length > 0)
                    {
                        var displayNode = CreateDisplayNode(token);
                        displayNode.NodeLine.NodeTypeDisplay = Styles.DisplayValueDisplay;
                        displayNode.NodeLine.DisplaySpanSource = TextSpanSource.Span;
                        children.Add(displayNode);
                    }

                    break;
                }
            }

            children.Sort(AnalysisTreeViewNodeBuilderObjectSpanComparer.Instance);

            if (Options.ShowSyntaxAnnotations)
            {
                children.Insert(0, CreateGreenNodeAnnotationsNode(token));
            }

            return children;
        }

        private void AppendTriviaListNode(
            SyntaxTriviaList triviaList,
            List<AnalysisTreeListNode> children,
            DisplayValueSource valueSource)
        {
            if (triviaList.Count is 0)
                return;

            var node = Creator.CreateRootTriviaList(triviaList, valueSource);
            children.Add(node);
        }

        private AnalysisTreeListNode CreateDisplayNode(SyntaxToken token)
        {
            return AnalysisTreeListNode(
                CreateDisplayNodeLine(token),
                () => CreatePropertyAnalysisChildren(token),
                token
            );
        }

        private IReadOnlyList<AnalysisTreeListNode> CreatePropertyAnalysisChildren(SyntaxToken token)
        {
            var text = token.Text;
            var valueText = token.ValueText;
            var value = token.Value;

            return
            [
                Creator.CreateNodeForSimplePropertyValue(text, nameof(SyntaxToken.Text)),
                Creator.CreateNodeForSimplePropertyValue(valueText, nameof(SyntaxToken.ValueText)),
                Creator.CreateNodeForSimplePropertyValue(value, nameof(SyntaxToken.Value))
            ];
        }

        private AnalysisTreeListNodeLine CreateDisplayNodeLine(SyntaxToken token)
        {
            var fullText = token.Text;
            var line = Creator.LineForNodeValue(
                fullText,
                default,
                Styles.DisplayValueDisplay);
            return line;
        }

        private SingleRunInline CreateDisplayTextRun(SyntaxToken token, SyntaxKind kind, bool isKeyword)
        {
            if (kind is SyntaxKind.EndOfFileToken)
            {
                return new(CreateEofRun());
            }

            if (kind is SyntaxKind.DocumentationCommentLineBreakToken)
            {
                var eolText = CreateDisplayStringForEndOfLineText(token.ToFullString());
                return new(Run(eolText, CommonStyles.RawValueBrush));
            }

            if (token.IsMissing)
            {
                return new(CreateMissingTokenRun());
            }

            var displayText = token.ToString();

            if (displayText.Length is 0)
            {
                return new(CreateEmptyValueRun());
            }

            var fullText = displayText;
            var displayBrush = isKeyword
                ? CommonStyles.KeywordBrush
                : CommonStyles.RawValueBrush;

            if (IsStringLiteralKind(kind))
            {
                displayText = Creator.SimplifyWhitespace(displayText);
            }

            var run = Run(displayText, displayBrush);
            return new SingleRunInline(run, fullText);
        }

        private static bool IsStringLiteralKind(SyntaxKind kind)
        {
            return kind
                is SyntaxKind.StringLiteralToken
                or SyntaxKind.StringLiteralExpression
                or SyntaxKind.InterpolatedStringTextToken

                // also handle XML texts
                or SyntaxKind.XmlText
                or SyntaxKind.XmlTextLiteralToken
                ;
        }

        private static AnalysisTreeListNode CreateEndOfFileDisplayNode(SyntaxToken token)
        {
            return AnalysisTreeListNode(
                CreateEndOfFileDisplayNodeLine(),
                null,
                token
            );
        }

        private static AnalysisTreeListNodeLine CreateEndOfFileDisplayNodeLine()
        {
            var eofRun = new SingleRunInline(CreateEofRun());

            return AnalysisTreeListNodeLine(
                [eofRun],
                Styles.DisplayValueDisplay);
        }

        private static Run CreateEofRun()
        {
            const string eofDisplayString = "[EOF]";
            return Run(eofDisplayString, Styles.EofBrush);
        }

        private static Run CreateMissingTokenRun()
        {
            const string missingTokenDisplayString = "[missing]";
            return Run(missingTokenDisplayString, Styles.MissingTokenIndicatorBrush);
        }

        private AnalysisTreeListNode CreateGreenNodeAnnotationsNode(SyntaxToken node)
        {
            var valueSource = CreateCommonSyntaxAnnotationsDisplayValue("Node");
            var annotations = RoslynInternalsEx.GetSyntaxAnnotations(node);
            return Creator.CreateRootSyntaxAnnotationList(annotations, valueSource);
        }
    }

    public sealed class SyntaxNodeListRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<ReadOnlySyntaxNodeList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            ReadOnlySyntaxNodeList list, DisplayValueSource valueSource)
        {
            var listType = list.GetType();
            if (listType.IsGenericType)
            {
                var genericDefinition = listType.GetGenericTypeDefinition();
                bool isSyntaxList =
                    genericDefinition == typeof(SyntaxList<>)
                    || genericDefinition == typeof(SeparatedSyntaxList<>)
                    ;

                if (isSyntaxList)
                {
                    return CreateBasicSyntaxListLine(list, valueSource);
                }
            }

            throw new ArgumentException("Invalid syntax node list type");
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(ReadOnlySyntaxNodeList value)
        {
            if (value.Count is 0)
                return null;

            return () => CreateNodeListChildren(value);
        }

        private AnalysisTreeListNodeLine CreateBasicSyntaxListLine(
            ReadOnlySyntaxNodeList list, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateSyntaxTypeInlines(list, valueSource);
            AppendCountValueDisplay(
                inlines,
                list.Count,
                nameof(ReadOnlySyntaxNodeList.Count));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SyntaxListNodeDisplay);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateNodeListChildren(ReadOnlySyntaxNodeList list)
        {
            var listType = list.GetType();
            if (listType.IsGenericType)
            {
                var genericDefinition = listType.GetGenericTypeDefinition();
                if (genericDefinition == typeof(SeparatedSyntaxList<>))
                {
                    // we use the underlying node or token list to display children
                    SyntaxNodeOrTokenList nodeOrTokenList = (list as dynamic)
                        .GetWithSeparators();
                    return Creator.CreateNodeListChildren(nodeOrTokenList);
                }
            }

            return list
                .Select(s => Creator.CreateRootNode(s))
                .ToList()
                ;
        }
    }

    public sealed class SyntaxTokenListRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTokenList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTokenList list, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateSyntaxTypeInlines(list, valueSource);
            AppendCountValueDisplay(
                inlines,
                list.Count,
                nameof(SyntaxTokenList.Count));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TokenListNodeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTokenList list)
        {
            if (list.Count is 0)
                return null;

            return () => CreateTokenListChildren(list);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateTokenListChildren(SyntaxTokenList list)
        {
            return list
                .Select(s => Creator.CreateRootToken(s))
                .ToList()
                ;
        }
    }

    public sealed class SyntaxTriviaRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTrivia>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTrivia trivia, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            AppendValueSource(valueSource, inlines);
            var display = FormatTriviaDisplay(trivia, inlines);

            return AnalysisTreeListNodeLine(inlines, display);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTrivia trivia)
        {
            var structure = trivia.GetStructure();
            if (structure is null)
            {
                return null;
            }

            var valueSource = MethodSource(nameof(SyntaxTrivia.GetStructure));
            var structureNode = Creator.CreateRootNode(structure, valueSource);
            return () => [structureNode];
        }

        private NodeTypeDisplay FormatTriviaDisplay(
            SyntaxTrivia trivia, GroupedRunInlineCollection inlines)
        {
            var structure = trivia.GetStructure();
            if (structure is null)
            {
                return FormatUnstructuredTriviaDisplay(trivia, inlines);
            }

            return FormatStructuredTriviaDisplay(trivia, inlines);
        }

        private NodeTypeDisplay FormatUnstructuredTriviaDisplay(
            SyntaxTrivia trivia, GroupedRunInlineCollection inlines)
        {
            var kind = trivia.Kind();
            switch (kind)
            {
                case SyntaxKind.WhitespaceTrivia:
                {
                    var displayText = WhitespaceTriviaText(trivia);
                    var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                    inlines.AddSingle(displayTextRun);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.WhitespaceTriviaKindBrush,
                        inlines);

                    return Styles.WhitespaceTriviaDisplay;
                }
                case SyntaxKind.ColonTrivia:
                case SyntaxKind.LineContinuationTrivia:
                {
                    var displayText = CommentTriviaText(trivia, out var fullString);
                    var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                    var group = new SingleRunInline(displayTextRun, fullString);
                    inlines.Add(group);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.WhitespaceTriviaKindBrush,
                        inlines);

                    return Styles.SplitterTriviaDisplay;
                }
                case SyntaxKind.CommentTrivia:
                case SyntaxKind.DocumentationCommentTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                case SyntaxKind.SkippedTokensTrivia:
                case SyntaxKind.ConflictMarkerTrivia:
                {
                    var displayText = CommentTriviaText(trivia, out var fullString);
                    var displayTextRun = Run(displayText, Styles.CommentTriviaContentBrush);
                    var group = new SingleRunInline(displayTextRun, fullString);
                    inlines.Add(group);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.CommentTriviaTokenKindBrush,
                        inlines);

                    return Styles.CommentTriviaDisplay;
                }
                case SyntaxKind.BadDirectiveTrivia:
                {
                    var displayText = CommentTriviaText(trivia, out var fullString);
                    var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                    var group = new SingleRunInline(displayTextRun, fullString);
                    inlines.Add(group);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.WhitespaceTriviaKindBrush,
                        inlines);

                    return Styles.DirectiveTriviaDisplay;
                }
                case SyntaxKind.DisabledTextTrivia:
                {
                    var disabledText = DisabledTextTriviaText(trivia);
                    var disabledTextRun = Run(disabledText, Styles.DisabledTextTriviaContentBrush);
                    inlines.AddSingle(disabledTextRun);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.DisabledTextTriviaTokenKindBrush,
                        inlines);

                    return Styles.DisabledTextTriviaDisplay;
                }
                case SyntaxKind.EndOfLineTrivia:
                {
                    var eolText = EndOfLineTriviaText(trivia);
                    var displayTextRun = Run(eolText, Styles.WhitespaceTriviaBrush);
                    inlines.AddSingle(displayTextRun);

                    AddTriviaKindWithSplitter(
                        trivia,
                        Styles.WhitespaceTriviaKindBrush,
                        inlines);

                    return Styles.EndOfLineTriviaDisplay;
                }
            }

            throw new ArgumentException("Unexpected trivia syntax kind");
        }

        private NodeTypeDisplay FormatStructuredTriviaDisplay(
            SyntaxTrivia trivia, GroupedRunInlineCollection inlines)
        {
            var kind = trivia.Kind();
            bool isDirective = SyntaxFacts.IsPreprocessorDirective(kind);
            if (isDirective)
            {
                var displayText = CommentTriviaText(trivia, out var fullString);
                var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                var group = new SingleRunInline(displayTextRun, fullString);
                inlines.Add(group);

                AddTriviaKindWithSplitter(
                    trivia,
                    Styles.WhitespaceTriviaKindBrush,
                    inlines);

                return Styles.DirectiveTriviaDisplay;
            }

            switch (kind)
            {
                case SyntaxKind.DocumentationCommentTrivia:
                {
                    var displayText = trivia.Kind().ToString();
                    var displayTextRun = Run(displayText, Styles.CommentTriviaContentBrush);
                    inlines.AddSingle(displayTextRun);

                    return Styles.CommentTriviaDisplay;
                }

                case SyntaxKind.SkippedTokensTrivia:
                {
                    var displayText = trivia.Kind().ToString();
                    var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                    inlines.AddSingle(displayTextRun);

                    return Styles.WhitespaceTriviaDisplay;
                }
            }

            Debug.Assert(false, "Unreachable by unknown trivia display");
            return Styles.WhitespaceTriviaDisplay;
        }

        private static string EndOfLineTriviaText(SyntaxTrivia trivia)
        {
            var text = trivia.ToFullString();
            return CreateDisplayStringForEndOfLineText(text);
        }

        private static string DisabledTextTriviaText(SyntaxTrivia trivia)
        {
            var span = trivia.Span;
            var lineSpan = trivia.SyntaxTree!.GetLineSpan(span).Span;
            int startLine = lineSpan.Start.Line + 1;
            int endLine = lineSpan.End.Line + 1;
            if (lineSpan.End.Character is 0)
            {
                endLine--;
            }

            if (startLine == endLine)
            {
                return $"[Line {startLine}]";
            }

            return $"[Lines {startLine} - {endLine}]";
        }

        private static string WhitespaceTriviaText(SyntaxTrivia trivia)
        {
            var resultString = string.Empty;

            var text = trivia.ToFullString();
            int length = text.Length;

            var currentWhitespace = WhitespaceKind.None;
            int currentWhitespaceRepeats = 0;

            for (int i = 0; i < length; i++)
            {
                var c = text[i];
                var iteratedWhitespace = c.GetWhitespaceKind();
                if (iteratedWhitespace != currentWhitespace)
                {
                    ConsumeCurrentStreak();
                    currentWhitespace = iteratedWhitespace;
                    currentWhitespaceRepeats = 0;
                }

                currentWhitespaceRepeats++;
            }

            ConsumeCurrentStreak();
            return resultString;

            void ConsumeCurrentStreak()
            {
                const string space = "space";
                const string tab = "tab";

                if (currentWhitespace is WhitespaceKind.None)
                    return;

                if (currentWhitespaceRepeats is 0)
                    return;

                var whitespaceKindDisplay = currentWhitespace switch
                {
                    WhitespaceKind.Space => space,
                    WhitespaceKind.Tab => tab,
                    _ => throw new UnreachableException(),
                };

                string appendString;
                if (currentWhitespaceRepeats is 1)
                {
                    appendString = $"[{whitespaceKindDisplay}]";
                }
                else
                {
                    appendString = $"[{currentWhitespaceRepeats}x {whitespaceKindDisplay}]";
                }

                resultString += appendString;
            }
        }

        private static void AddTriviaKindWithSplitter(
            SyntaxTrivia trivia,
            ILazilyUpdatedBrush brush,
            GroupedRunInlineCollection inlines)
        {
            inlines.Add(NewValueKindSplitterRun());

            var triviaKindText = trivia.Kind().ToString();
            var triviaKindRun = Run(triviaKindText, brush, FontStyle.Italic);
            inlines.AddSingle(triviaKindRun);
        }

        private string CommentTriviaText(SyntaxTrivia trivia, out string fullString)
        {
            fullString = trivia.ToFullString();
            return Creator.SimplifyWhitespace(fullString);
        }
    }

    public sealed class SyntaxTriviaListRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTriviaList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTriviaList list, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateSyntaxTypeInlines(list, valueSource);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TriviaListDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTriviaList list)
        {
            if (list.Count is 0)
                return null;

            return () => CreateTriviaListChildren(list);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateTriviaListChildren(SyntaxTriviaList list)
        {
            var children = list
                .Select(s => Creator.CreateRootTrivia(s))
                .ToList()
                ;

            if (Options.ShowSyntaxAnnotations)
            {
                children.Insert(0, CreateGreenNodeAnnotationsNode(list));
            }

            return children;
        }

        private AnalysisTreeListNode CreateGreenNodeAnnotationsNode(SyntaxTriviaList triviaList)
        {
            var valueSource = CreateCommonSyntaxAnnotationsDisplayValue("Node");
            var annotations = RoslynInternalsEx.GetSyntaxAnnotations(triviaList);
            return Creator.CreateRootSyntaxAnnotationList(annotations, valueSource);
        }
    }

    public sealed class SyntaxReferenceRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxReference>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxReference reference, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            var inline = NestedTypeDisplayGroupedRun(typeof(SyntaxReference));
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SyntaxReferenceDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            SyntaxReference reference)
        {
            return () => GetChildren(reference);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SyntaxReference reference)
        {
            return
            [
                Creator.CreateRootTextSpan(
                    reference.Span!,
                    Property(nameof(SyntaxReference.Span))),

                Creator.CreateRootNode(
                    reference.GetSyntax(),
                    MethodSource(nameof(SyntaxReference.GetSyntax)))!,
            ];
        }
    }

    public sealed class TextSpanRootViewNodeCreator(VisualBasicSyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<TextSpan>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TextSpan span, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            var inline = NestedTypeDisplayGroupedRun(typeof(TextSpan));
            inlines.Add(inline);
            inlines.Add(NewValueKindSplitterRun());
            var spanView = CreateSpanViewRun(span);
            inlines.Add(spanView);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TextSpanDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            TextSpan span)
        {
            return null;
        }

        private static ComplexGroupedRunInline CreateSpanViewRun(TextSpan span)
        {
            var startRun = new SingleRunInline(
                Run(span.Start.ToString(), CommonStyles.RawValueBrush));

            var rangeSplitterRun = Run("..", CommonStyles.SplitterBrush);

            var endRun = new SingleRunInline(
                Run(span.End.ToString(), CommonStyles.RawValueBrush));

            var leftParenthesisRun = Run("  (", CommonStyles.SplitterBrush);
            var lengthRun = new SingleRunInline(
                Run(span.Length.ToString(), CommonStyles.RawValueBrush));
            var rightParenthesisRun = Run(")", CommonStyles.SplitterBrush);

            return new ComplexGroupedRunInline(
            [
                new RunOrGrouped(
                    new ComplexGroupedRunInline(
                    [
                        new(startRun),
                        rangeSplitterRun,
                        new(endRun),
                    ])),
                leftParenthesisRun,
                new(lengthRun),
                rightParenthesisRun,
            ]);
        }
    }
}
