using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed partial class SyntaxAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    private static readonly InterestingPropertyFilterCache _propertyCache
        = new(SyntaxNodePropertyFilter.Instance);

    // node creators
    private readonly SyntaxTreeRootViewNodeCreator _treeCreator;
    private readonly NodeOrTokenRootViewNodeCreator _nodeOrTokenCreator;
    private readonly SyntaxNodeRootViewNodeCreator _syntaxNodeCreator;
    private readonly SyntaxTokenRootViewNodeCreator _syntaxTokenCreator;
    private readonly SyntaxNodeListRootViewNodeCreator _syntaxNodeListCreator;
    private readonly SyntaxTokenListRootViewNodeCreator _syntaxTokenListCreator;
    private readonly SyntaxTriviaRootViewNodeCreator _syntaxTriviaCreator;
    private readonly SyntaxTriviaListRootViewNodeCreator _syntaxTriviaListCreator;

    public SyntaxAnalysisNodeCreator(AnalysisNodeCreationOptions options)
        : base(options)
    {
        _treeCreator = new(this);
        _nodeOrTokenCreator = new(this);
        _syntaxNodeCreator = new(this);
        _syntaxTokenCreator = new(this);
        _syntaxNodeListCreator = new(this);
        _syntaxTokenListCreator = new(this);
        _syntaxTriviaCreator = new(this);
        _syntaxTriviaListCreator = new(this);
    }

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
        }

        return null;
    }

    public AnalysisTreeListNode CreateRootTree(
        SyntaxTree tree, DisplayValueSource valueSource = default)
    {
        return _treeCreator.CreateNode(tree, valueSource);
    }

    public AnalysisTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, DisplayValueSource valueSource = default)
    {
        return _nodeOrTokenCreator.CreateNode(nodeOrToken, valueSource);
    }

    public AnalysisTreeListNode CreateRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default)
    {
        return _syntaxNodeCreator.CreateNode(node, valueSource);
    }

    public AnalysisTreeListNode CreateRootToken(
        SyntaxToken token, DisplayValueSource valueSource = default)
    {
        return _syntaxTokenCreator.CreateNode(token, valueSource);
    }

    public AnalysisTreeListNode CreateRootNodeList(
        ReadOnlySyntaxNodeList node, DisplayValueSource valueSource = default)
    {
        return _syntaxNodeListCreator.CreateNode(node, valueSource);
    }

    private void AppendTriviaList(SyntaxTriviaList triviaList, List<AnalysisTreeListNode> children)
    {
        if (Options.ShowTrivia)
        {
            foreach (var trivia in triviaList)
            {
                var triviaNode = CreateRootTrivia(trivia);
                children.Add(triviaNode);
            }
        }
    }

    private AnalysisTreeListNode CreateRootTokenList(
        SyntaxTokenList list, DisplayValueSource valueSource)
    {
        return _syntaxTokenListCreator.CreateNode(list, valueSource);
    }

    private IReadOnlyList<AnalysisTreeListNode> CreateNodeListChildren(SyntaxNodeOrTokenList list)
    {
        return list
            .Select(s => CreateRootNodeOrToken(s))
            .ToList()
            ;
    }

    private static IReadOnlyList<PropertyInfo> GetInterestingPropertiesForNodeType(SyntaxNode node)
    {
        var nodeType = node.GetType();
        var properties = _propertyCache.FilterForType(nodeType).Properties;
        return properties;
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

    private GroupedRunInlineCollection CreateBasicTypeNameInlines(
        object value, DisplayValueSource valueSource)
    {
        var inlines = new GroupedRunInlineCollection();
        var type = value.GetType();

        AppendValueSource(valueSource, inlines);
        AppendTypeDetails(type, inlines);

        return inlines;
    }

    public AnalysisTreeListNode CreateRootTrivia(
        SyntaxTrivia trivia, DisplayValueSource valueSource = default)
    {
        return _syntaxTriviaCreator.CreateNode(trivia, valueSource);
    }

    public AnalysisTreeListNode CreateRootTriviaList(
        SyntaxTriviaList triviaList, DisplayValueSource valueSource)
    {
        return _syntaxTriviaListCreator.CreateNode(triviaList, valueSource);
    }

    private void AppendTypeDetails(Type type, GroupedRunInlineCollection inlines)
    {
        var typeNameRun = TypeDetailsGroupedRun(type);
        inlines.Add(typeNameRun);
    }

    private GroupedRunInline TypeDetailsGroupedRun(Type type)
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
                var inner = TypeDetailsGroupedRun(argument);

                return new ComplexGroupedRunInline([
                    outerRun,
                    inner,
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
        if (typeName.EndsWith(syntaxSuffix))
        {
            var primaryClassNameRun = Run(typeName[..^syntaxSuffix.Length], Styles.ClassMainBrush);
            var suffixNameRun = Run(syntaxSuffix, Styles.ClassSecondaryBrush);

            return new SimpleGroupedRunInline([
                primaryClassNameRun,
                suffixNameRun,
            ]);
        }

        return CreateBasicClassInline(typeName);
    }
}

partial class SyntaxAnalysisNodeCreator
{
    public abstract class SyntaxRootViewNodeCreator<TValue>(SyntaxAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, SyntaxAnalysisNodeCreator>(creator);

    public sealed class SyntaxTreeRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTree>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTree tree, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateBasicTypeNameInlines(tree, valueSource);

            var language = tree.GetLanguage();

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = DisplayForLanguage(language),
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTree tree)
        {
            return () => GetChildren(tree);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            return [
                Creator.CreateRootNode(root, MethodSource(nameof(SyntaxTree.GetRoot))),

                // Uncomment once the generic display is ready
                //Creator.CreateRootGeneric(tree.Options, Property(nameof(SyntaxTree.Options))),
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
                _ => throw new ArgumentException("Invalid language"),
            };
        }
    }

    public sealed class NodeOrTokenRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
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

    public sealed class SyntaxNodeRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxNode>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxNode node, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateBasicTypeNameInlines(node, valueSource);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = Styles.ClassNodeDisplay,
            };
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
                bool includeValue = Creator.ShouldIncludeValue(value);
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

            children.Sort(AnalysisTreeViewNodeObjectSpanComparer.Instance);

            return children;
        }
    }

    public sealed class SyntaxTokenRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxToken>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxToken token, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            AppendTokenKindDetails(token, valueSource.Name, inlines);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = Styles.TokenNodeDisplay,
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.XmlTextLiteralNewLineToken:
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
            var kindRun = Run(kindName, kindBrush);
            kindRun.FontStyle = FontStyle.Italic;

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
                        children.Add(displayNode);
                    }

                    break;
                }
            }

            children.Sort(AnalysisTreeViewNodeObjectSpanComparer.Instance);

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
            return new()
            {
                NodeLine = CreateDisplayNodeLine(token),
                ChildRetriever = () => CreatePropertyAnalysisChildren(token),
                AssociatedSyntaxObjectContent = token,
            };
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
            var line = Creator.LineForNodeValue(fullText);
            line.NodeTypeDisplay = Styles.DisplayValueDisplay;
            return line;
        }

        private SingleRunInline CreateDisplayTextRun(SyntaxToken token, SyntaxKind kind, bool isKeyword)
        {
            if (kind is SyntaxKind.EndOfFileToken)
            {
                return new(CreateEofRun());
            }

            if (kind is SyntaxKind.XmlTextLiteralNewLineToken)
            {
                var eolText = CreateDisplayStringForEndOfLineText(token.ToFullString());
                return new(Run(eolText, Styles.RawValueBrush));
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
                ? Styles.KeywordBrush
                : Styles.RawValueBrush;

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
                or SyntaxKind.MultiLineRawStringLiteralToken
                or SyntaxKind.SingleLineRawStringLiteralToken
                or SyntaxKind.Utf8StringLiteralToken
                or SyntaxKind.Utf8MultiLineRawStringLiteralToken
                or SyntaxKind.Utf8SingleLineRawStringLiteralToken

                // also handle XML texts
                or SyntaxKind.XmlText
                or SyntaxKind.XmlTextLiteralToken
                ;
        }

        private static AnalysisTreeListNode CreateEndOfFileDisplayNode(SyntaxToken token)
        {
            return new()
            {
                NodeLine = CreateEndOfFileDisplayNodeLine(),
                AssociatedSyntaxObjectContent = token,
            };
        }

        private static AnalysisTreeListNodeLine CreateEndOfFileDisplayNodeLine()
        {
            var eofRun = new SingleRunInline(CreateEofRun());

            return new()
            {
                GroupedRunInlines = [eofRun],
                NodeTypeDisplay = Styles.DisplayValueDisplay,
            };
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
    }

    public sealed class SyntaxNodeListRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
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
            ReadOnlySyntaxNodeList node, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateBasicTypeNameInlines(node, valueSource);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
            };
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

    public sealed class SyntaxTokenListRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTokenList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTokenList list, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateBasicTypeNameInlines(list, valueSource);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = Styles.TokenListNodeDisplay,
            };
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

    public sealed class SyntaxTriviaRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTrivia>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTrivia trivia, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var display = FormatTriviaDisplay(trivia, inlines);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = display,
            };
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
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
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
                case SyntaxKind.PreprocessingMessageTrivia:
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
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
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
            IBrush brush,
            GroupedRunInlineCollection inlines)
        {
            inlines.Add(NewValueKindSplitterRun());

            var triviaKindText = trivia.Kind().ToString();
            var triviaKindRun = Run(triviaKindText, brush);
            triviaKindRun.FontStyle = FontStyle.Italic;
            inlines.AddSingle(triviaKindRun);
        }

        private string CommentTriviaText(SyntaxTrivia trivia, out string fullString)
        {
            fullString = trivia.ToFullString();
            return Creator.SimplifyWhitespace(fullString);
        }
    }

    public sealed class SyntaxTriviaListRootViewNodeCreator(SyntaxAnalysisNodeCreator creator)
        : SyntaxRootViewNodeCreator<SyntaxTriviaList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SyntaxTriviaList list, DisplayValueSource valueSource)
        {
            var inlines = Creator.CreateBasicTypeNameInlines(list, valueSource);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = Styles.TriviaListDisplay,
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SyntaxTriviaList list)
        {
            if (list.Count is 0)
                return null;

            return () => CreateTriviaListChildren(list);
        }

        private IReadOnlyList<AnalysisTreeListNode> CreateTriviaListChildren(SyntaxTriviaList list)
        {
            return list
                .Select(s => Creator.CreateRootTrivia(s))
                .ToList()
                ;
        }
    }
}

partial class SyntaxAnalysisNodeCreator
{
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

        public const string WhitespaceTrivia = "_";
        public const string CommentTrivia = "/*";
        public const string DirectiveTrivia = "#";
        public const string DisabledTextTrivia = "~";
        public const string EndOfLineTrivia = @"\n";
    }

    public abstract class Styles : CommonStyles
    {
        public static readonly Color CSharpTreeColor = Color.FromUInt32(0xFF33E5A5);
        public static readonly SolidColorBrush CSharpTreeBrush = new(CSharpTreeColor);

        public static readonly Color VisualBasicTreeColor = Color.FromUInt32(0xFF74A3FF);
        public static readonly SolidColorBrush VisualBasicTreeBrush = new(VisualBasicTreeColor);

        public static readonly Color WhitespaceTriviaColor = Color.FromUInt32(0xFFB3B3B3);
        public static readonly SolidColorBrush WhitespaceTriviaBrush = new(WhitespaceTriviaColor);

        public static readonly Color WhitespaceTriviaKindColor = Color.FromUInt32(0xFF808080);
        public static readonly SolidColorBrush WhitespaceTriviaKindBrush = new(WhitespaceTriviaKindColor);

        public static readonly Color TokenKindColor = Color.FromUInt32(0xFF7A68E5);
        public static readonly SolidColorBrush TokenKindBrush = new(TokenKindColor);

        public static readonly Color FadeTokenKindColor = Color.FromUInt32(0xFF514599);
        public static readonly SolidColorBrush FadeTokenKindBrush = new(FadeTokenKindColor);

        public static readonly Color TokenListColor = Color.FromUInt32(0xFF74A3FF);
        public static readonly SolidColorBrush TokenListBrush = new(TokenListColor);

        public static readonly Color SyntaxListColor = Color.FromUInt32(0xFF79BCA4);
        public static readonly SolidColorBrush SyntaxListBrush = new(SyntaxListColor);

        public static readonly Color EofColor = Color.FromUInt32(0xFF76788B);
        public static readonly SolidColorBrush EofBrush = new(EofColor);

        public static readonly Color BasicTriviaNodeTypeColor = Color.FromUInt32(0xFF7C7C7C);
        public static readonly SolidColorBrush BasicTriviaNodeTypeBrush = new(BasicTriviaNodeTypeColor);

        public static readonly Color WhitespaceTriviaNodeTypeColor = Color.FromUInt32(0xFF7C7C7C);
        public static readonly SolidColorBrush WhitespaceTriviaNodeTypeBrush = new(WhitespaceTriviaNodeTypeColor);

        public static readonly Color TriviaListTypeColor = Color.FromUInt32(0xFF7C7C7C);
        public static readonly SolidColorBrush TriviaListTypeBrush = new(TriviaListTypeColor);

        public static readonly Color DisplayValueNodeTypeColor = Color.FromUInt32(0xFFCC935F);
        public static readonly SolidColorBrush DisplayValueNodeTypeBrush = new(DisplayValueNodeTypeColor);

        public static readonly Color CommentTriviaNodeTypeColor = Color.FromUInt32(0xFF00A858);
        public static readonly SolidColorBrush CommentTriviaNodeTypeBrush = new(CommentTriviaNodeTypeColor);

        public static readonly Color CommentTriviaContentColor = Color.FromUInt32(0xFF00703A);
        public static readonly SolidColorBrush CommentTriviaContentBrush = new(CommentTriviaContentColor);

        public static readonly Color CommentTriviaTokenKindColor = Color.FromUInt32(0xFF004D28);
        public static readonly SolidColorBrush CommentTriviaTokenKindBrush = new(CommentTriviaTokenKindColor);

        public static readonly Color DisabledTextTriviaNodeTypeColor = Color.FromUInt32(0xFF8B4D4D);
        public static readonly SolidColorBrush DisabledTextTriviaNodeTypeBrush = new(DisabledTextTriviaNodeTypeColor);

        public static readonly Color DisabledTextTriviaContentColor = Color.FromUInt32(0xFF664747);
        public static readonly SolidColorBrush DisabledTextTriviaContentBrush = new(DisabledTextTriviaContentColor);

        public static readonly Color DisabledTextTriviaTokenKindColor = Color.FromUInt32(0xFF4D3636);
        public static readonly SolidColorBrush DisabledTextTriviaTokenKindBrush = new(DisabledTextTriviaTokenKindColor);

        public static readonly Color MissingTokenIndicatorColor = Color.FromUInt32(0xFF8B4D4D);
        public static readonly SolidColorBrush MissingTokenIndicatorBrush = new(MissingTokenIndicatorColor);

        // Tree displays
        public static readonly NodeTypeDisplay CSharpTreeDisplay
            = new(Types.CSharpTree, CSharpTreeColor);

        public static readonly NodeTypeDisplay VisualBasicTreeDisplay
            = new(Types.VisualBasicTree, VisualBasicTreeColor);

        // Node displays
        public static readonly NodeTypeDisplay ClassNodeDisplay
            = new(Types.Node, ClassMainColor);

        public static readonly NodeTypeDisplay SyntaxListNodeDisplay
            = new(Types.SyntaxList, SyntaxListColor);

        public static readonly NodeTypeDisplay TokenListNodeDisplay
            = new(Types.TokenList, TokenListColor);

        public static readonly NodeTypeDisplay TokenNodeDisplay
            = new(Types.Token, TokenKindColor);

        public static readonly NodeTypeDisplay DisplayValueDisplay
            = new(Types.DisplayValue, DisplayValueNodeTypeColor);

        public static readonly NodeTypeDisplay TriviaListDisplay
            = new(Types.TriviaList, BasicTriviaNodeTypeColor);

        // Trivia displays
        public static readonly NodeTypeDisplay WhitespaceTriviaDisplay
            = new(Types.WhitespaceTrivia, WhitespaceTriviaColor);

        public static readonly NodeTypeDisplay DirectiveTriviaDisplay
            = new(Types.DirectiveTrivia, BasicTriviaNodeTypeColor);

        public static readonly NodeTypeDisplay EndOfLineTriviaDisplay
            = new(Types.EndOfLineTrivia, BasicTriviaNodeTypeColor);

        public static readonly NodeTypeDisplay CommentTriviaDisplay
            = new(Types.CommentTrivia, CommentTriviaNodeTypeColor);

        public static readonly NodeTypeDisplay DisabledTextTriviaDisplay
            = new(Types.DisabledTextTrivia, DisabledTextTriviaNodeTypeColor);
    }
}