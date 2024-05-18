﻿using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Syndiesis.Core.DisplayAnalysis;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed partial class SyntaxViewNodeLineCreator(NodeLineCreationOptions options)
    : BaseNodeLineCreator(options)
{
    private const string eofDisplayString = "[EOF]";
    private const string missingTokenDisplayString = "[Missing]";

    private static readonly InterestingPropertyFilterCache _propertyCache
        = new(SyntaxNodePropertyFilter.Instance);

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value, DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case SyntaxNodeOrToken nodeOrToken:
                return CreateRootNodeOrToken(nodeOrToken, valueSource);

            case SyntaxNode node:
                return CreateRootNode(node, valueSource);

            case SyntaxToken token:
                return CreateRootToken(token, valueSource);

            case ReadOnlySyntaxNodeList nodeList:
                return CreateRootNodeList(nodeList, valueSource);
        }

        return null;
    }

    public AnalysisTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, DisplayValueSource valueSource = default)
    {
        var rootLine = CreateNodeOrTokenLine(nodeOrToken, valueSource);
        var children = GetChildRetrieverForNodeOrToken(nodeOrToken);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = nodeOrToken,
        };
    }

    private Func<IReadOnlyList<AnalysisTreeListNode>>? GetChildRetrieverForNodeOrToken(
        SyntaxNodeOrToken nodeOrToken)
    {
        if (nodeOrToken.IsNode)
        {
            return GetChildRetrieverForNode(nodeOrToken.AsNode()!);
        }

        return GetChildRetrieverForToken(nodeOrToken.AsToken());
    }

    private AnalysisTreeListNodeLine CreateNodeOrTokenLine(
        SyntaxNodeOrToken nodeOrToken, DisplayValueSource valueSource)
    {
        if (nodeOrToken.IsNode)
        {
            return CreateNodeLine(nodeOrToken.AsNode()!, valueSource);
        }

        return CreateTokenNodeLine(nodeOrToken.AsToken(), valueSource);
    }

    public AnalysisTreeListNode CreateRootNode(
        SyntaxNode node, DisplayValueSource valueSource = default)
    {
        var rootLine = CreateNodeLine(node, valueSource);
        var children = GetChildRetrieverForNode(node);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = node,
        };
    }

    private Func<IReadOnlyList<AnalysisTreeListNode>>? GetChildRetrieverForNode(SyntaxNode node)
    {
        if (node.IsMissing && !node.HasAnyTrivia())
            return null;

        return () => CreateNodeChildren(node);
    }

    public AnalysisTreeListNode CreateRootToken(
        SyntaxToken token, DisplayValueSource valueSource = default)
    {
        var rootLine = CreateTokenNodeLine(token, valueSource);
        var children = GetChildRetrieverForToken(token);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = token,
        };
    }

    private Func<IReadOnlyList<AnalysisTreeListNode>>? GetChildRetrieverForToken(SyntaxToken token)
    {
        switch (token.Kind())
        {
            case SyntaxKind.XmlTextLiteralNewLineToken:
                return null;
        }

        if (token.IsMissing && !token.HasAnyTrivia())
            return null;

        return () => CreateTokenChildren(token);
    }

    public AnalysisTreeListNode CreateRootNodeList(
        ReadOnlySyntaxNodeList node, DisplayValueSource valueSource = default)
    {
        var rootLine = CreateSyntaxListLine(node, valueSource);
        var children = () => CreateNodeListChildren(node);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = node,
        };
    }

    private IReadOnlyList<AnalysisTreeListNode> CreateTokenChildren(SyntaxToken token)
    {
        int triviaCount = 0;
        SyntaxTriviaList leadingTrivia = default;
        SyntaxTriviaList trailingTrivia = default;
        if (Options.ShowTrivia)
        {
            leadingTrivia = token.LeadingTrivia;
            trailingTrivia = token.TrailingTrivia;
            triviaCount = leadingTrivia.Count + trailingTrivia.Count;
        }
        var children = new List<AnalysisTreeListNode>(triviaCount + 1);

        // they will be sorted anyway
        if (Options.ShowTrivia)
        {
            AppendTriviaList(leadingTrivia, children);
            AppendTriviaList(trailingTrivia, children);
        }

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
                if (token.ValueText.Length > 0)
                {
                    var displayNode = CreateDisplayNode(token);
                    children.Add(displayNode);
                }
                break;
            }
        }

        children.Sort(SyntaxTreeViewNodeObjectSpanComparer.Instance);

        return children;
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
        return Run(eofDisplayString, Styles.EofBrush);
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

    private AnalysisTreeListNodeLine CreateDisplayNodeLine(SyntaxToken token)
    {
        var fullText = token.Text;
        string text = DisplayStringForText(token, fullText);
        return LineForNodeValue(text, fullText);
    }

    private string DisplayStringForText(SyntaxToken token, string text)
    {
        if (IsStringLiteralKind(token.Kind()))
        {
            text = SimplifyWhitespace(text);
        }

        return text;
    }

    private IReadOnlyList<AnalysisTreeListNode> CreatePropertyAnalysisChildren(SyntaxToken token)
    {
        var fullText = token.Text;
        string text = DisplayStringForText(token, fullText);
        var textLine = CreateNodeForNodeValue(
            text,
            fullText,
            Property(nameof(SyntaxToken.Text)));

        var fullValueText = token.ValueText;
        string valueText = DisplayStringForText(token, fullValueText);
        var valueTextLine = CreateNodeForNodeValue(
            valueText,
            fullValueText,
            Property(nameof(SyntaxToken.ValueText)));

        var value = token.Value;
        var fullValue = value;
        if (value is string valueString)
        {
            value = DisplayStringForText(token, valueString);
        }
        var valueLine = CreateNodeForNodeValue(
            value,
            fullValue,
            Property(nameof(SyntaxToken.Value)));

        return
        [
            textLine,
            valueTextLine,
            valueLine
        ];
    }

    private AnalysisTreeListNode CreateNodeForNodeValue(
        object? value,
        object? fullValue,
        DisplayValueSource valueSource = default)
    {
        var line = LineForNodeValue(value, fullValue, valueSource);
        line.NodeTypeDisplay = Styles.PropertyAnalysisValueDisplay;
        return new()
        {
            NodeLine = line,
        };
    }

    private AnalysisTreeListNodeLine LineForNodeValue(
        object? value, object? fullValue, DisplayValueSource valueSource = default)
    {
        var inlines = new GroupedRunInlineCollection();

        AppendValueSource(valueSource, inlines);
        var valueRun = RunForObjectValue(value, fullValue);
        inlines.Add(valueRun);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.DisplayValueDisplay,
        };
    }

    private static SingleRunInline RunForObjectValue(object? value, object? fullValue)
    {
        if (value is null)
            return new SingleRunInline(CreateNullValueRun());

        var text = value.ToString()!;
        var fullText = fullValue!.ToString()!;
        var run = Run(text, Styles.RawValueBrush);
        return new SingleRunInline(run, fullText);
    }

    private static Run CreateNullValueRun()
    {
        const string nullDisplay = "[null]";
        var run = Run(nullDisplay, Styles.WhitespaceTriviaKindBrush);
        run.FontStyle = FontStyle.Italic;
        return run;
    }

    private void AppendTriviaList(SyntaxTriviaList triviaList, List<AnalysisTreeListNode> children)
    {
        if (Options.ShowTrivia)
        {
            foreach (var trivia in triviaList)
            {
                var triviaNode = CreateTriviaNode(trivia);
                children.Add(triviaNode);
            }
        }
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

            var childNode = CreateRootViewNode(value, valueSource);
            if (childNode is not null)
            {
                children.Add(childNode);
            }
        }

        children.Sort(SyntaxTreeViewNodeObjectSpanComparer.Instance);

        return children;
    }

    private AnalysisTreeListNode CreateTokenListNode(
        SyntaxTokenList list, DisplayValueSource valueSource)
    {
        var node = CreateTokenListNodeLine(list, valueSource);
        var children = () => CreateTokenListChildren(list);

        return new()
        {
            NodeLine = node,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = list,
        };
    }

    private AnalysisTreeListNodeLine CreateTokenListNodeLine(
        SyntaxTokenList list, DisplayValueSource valueSource)
    {
        var inlines = CreateBasicTypeNameInlines(list, valueSource);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.TokenListNodeDisplay,
        };
    }

    private IReadOnlyList<AnalysisTreeListNode> CreateTokenListChildren(SyntaxTokenList list)
    {
        return list
            .Select(s => CreateRootToken(s))
            .ToList()
            ;
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
                return CreateNodeListChildren(nodeOrTokenList);
            }
        }

        return list
            .Select(s => CreateRootNode(s))
            .ToList()
            ;
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

    public AnalysisTreeListNodeLine CreateNodeLine(
        SyntaxNode node, DisplayValueSource valueSource = default)
    {
        var inlines = CreateBasicTypeNameInlines(node, valueSource);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.ClassNodeDisplay,
        };
    }

    public AnalysisTreeListNodeLine CreateSyntaxListLine(
        ReadOnlySyntaxNodeList list, DisplayValueSource valueSource = default)
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

            var typeArgument = listType.GenericTypeArguments[0];

            // keep this until it's hopefully not used
            if (genericDefinition == typeof(SyntaxList<>))
            {
                const string methodName = nameof(CreateSyntaxListLine);
                var invokedMethod = this.GetType()
                    .GetMethod(nameof(methodName))
                    !.MakeGenericMethod([typeArgument]);
                var result = invokedMethod.Invoke(this, [list, valueSource])
                    as AnalysisTreeListNodeLine;
                return result!;
            }
            if (genericDefinition == typeof(SeparatedSyntaxList<>))
            {
                const string methodName = nameof(CreateSeparatedSyntaxListLine);
                var invokedMethod = this.GetType()
                    .GetMethod(nameof(methodName))
                    !.MakeGenericMethod([typeArgument]);
                var result = invokedMethod.Invoke(this, [list, valueSource])
                    as AnalysisTreeListNodeLine;
                return result!;
            }
        }

        throw new ArgumentException("Invalid syntax node list type");
    }

    public AnalysisTreeListNodeLine CreateBasicSyntaxListLine(
        ReadOnlySyntaxNodeList node, DisplayValueSource valueSource)
    {
        var inlines = CreateBasicTypeNameInlines(node, valueSource);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
    }

    public AnalysisTreeListNodeLine CreateSyntaxListLine<T>(
        SyntaxList<T> node, DisplayValueSource valueSource = default)
        where T : SyntaxNode
    {
        var inlines = CreateBasicTypeNameInlines(node, valueSource);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
    }

    public AnalysisTreeListNodeLine CreateSeparatedSyntaxListLine<T>(
        SeparatedSyntaxList<T> node, DisplayValueSource valueSource)
        where T : SyntaxNode
    {
        var inlines = CreateBasicTypeNameInlines(node, valueSource);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
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

    public AnalysisTreeListNodeLine CreateTokenNodeLine(
        SyntaxToken token, DisplayValueSource valueSource)
    {
        var inlines = new GroupedRunInlineCollection();

        AppendValueSource(valueSource, inlines);
        AppendTokenKindDetails(token, valueSource.Name, inlines);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = Styles.TokenNodeDisplay,
        };
    }

    public AnalysisTreeListNode CreateTriviaNode(SyntaxTrivia trivia)
    {
        var line = CreateTriviaLine(trivia);
        var structure = trivia.GetStructure();
        Func<IReadOnlyList<AnalysisTreeListNode>>? children = null;
        if (structure is not null)
        {
            var valueSource = new DisplayValueSource(
                DisplayValueSource.SymbolKind.Method,
                nameof(SyntaxTrivia.GetStructure));
            var structureNode = CreateRootNode(structure, valueSource);
            children = () => [structureNode];
        }

        return new()
        {
            NodeLine = line,
            AssociatedSyntaxObjectContent = trivia,
            ChildRetriever = children,
        };
    }

    public AnalysisTreeListNodeLine CreateTriviaLine(SyntaxTrivia trivia)
    {
        var inlines = new GroupedRunInlineCollection();

        var display = FormatTriviaDisplay(trivia, inlines);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = display,
        };
    }

    private NodeTypeDisplay FormatTriviaDisplay(SyntaxTrivia trivia, GroupedRunInlineCollection inlines)
    {
        var structure = trivia.GetStructure();
        if (structure is null)
        {
            return FormatUnstructuredTriviaDisplay(trivia, inlines);
        }

        return FormatStructuredTriviaDisplay(trivia, inlines);
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

    private static string EndOfLineTriviaText(SyntaxTrivia trivia)
    {
        var text = trivia.ToFullString();
        return CreateDisplayStringForEndOfLineText(text);
    }

    private static string CreateDisplayStringForEndOfLineText(string text)
    {
        switch (text)
        {
            case "\r\n":
                return """\r\n""";
            case "\r":
                return """\r""";
            case "\n":
                return """\n""";
        }

        // do not bother escaping the unusual EOL trivia token we receive;
        // handle this another time
        return text;
    }

    private string CommentTriviaText(SyntaxTrivia trivia, out string fullString)
    {
        fullString = trivia.ToFullString();
        return SimplifyWhitespace(fullString);
    }

    private string SimplifyWhitespace(string source)
    {
        return SimplifyWhitespace(source, Options.TruncationLimit);
    }

    private static string SimplifyWhitespace(string source, int truncationLength)
    {
        var trimmedText = new StringBuilder(source.Length);
        bool hasWhitespace = false;

        for (int i = 0; i < source.Length; i++)
        {
            var c = source[i];
            switch (c)
            {
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                {
                    if (hasWhitespace)
                    {
                        continue;
                    }

                    hasWhitespace = true;
                    trimmedText.Append(' ');

                    break;
                }

                default:
                    hasWhitespace = false;
                    trimmedText.Append(c);
                    break;
            }

            const string truncationSuffix = "...";
            int remaining = source.Length - i;
            if (trimmedText.Length >= truncationLength && remaining > truncationSuffix.Length)
            {
                trimmedText.Append(truncationSuffix);
                break;
            }
        }

        return trimmedText.ToString();
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

    private void AppendTokenKindDetails(SyntaxToken token, string? propertyName, GroupedRunInlineCollection inlines)
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
        var fullText = displayText;
        var displayBrush = isKeyword
            ? Styles.KeywordBrush
            : Styles.RawValueBrush;

        if (IsStringLiteralKind(kind))
        {
            displayText = SimplifyWhitespace(displayText);
        }

        var run = Run(displayText, displayBrush);
        return new SingleRunInline(run, fullText);
    }

    private static Run CreateMissingTokenRun()
    {
        return Run(missingTokenDisplayString, Styles.MissingTokenIndicatorBrush);
    }

    private static Run NewValueKindSplitterRun()
    {
        return Run("      ", Styles.RawValueBrush);
    }

    private void AppendValueSource(
        DisplayValueSource valueSource,
        GroupedRunInlineCollection inlines)
    {
        if (valueSource.IsDefault)
            return;

        switch (valueSource.Kind)
        {
            case DisplayValueSource.SymbolKind.Property:
                AppendPropertyDetail(valueSource.Name!, inlines);
                break;

            case DisplayValueSource.SymbolKind.Method:
                AppendMethodDetail(valueSource, inlines);
                break;
        }
    }

    private void AppendMethodDetail(
        DisplayValueSource valueSource, GroupedRunInlineCollection inlines)
    {
        var propertyNameRun = Run(valueSource.Name!, Styles.MethodBrush);
        var parenthesesRun = Run("()", Styles.RawValueBrush);
        var frontGroup = new SimpleGroupedRunInline([
            propertyNameRun,
            parenthesesRun,
        ]);
        var colonRun = Run(":  ", Styles.SplitterBrush);
        inlines.AddRange([
            frontGroup,
            colonRun
        ]);
    }

    private void AppendPropertyDetail(string propertyName, GroupedRunInlineCollection inlines)
    {
        var propertyNameRun = Run(propertyName, Styles.PropertyBrush);
        var colonRun = Run(":  ", Styles.SplitterBrush);
        inlines.AddSingle(propertyNameRun);
        inlines.Add(colonRun);
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
                // remove the `1 suffix
                string name = originalDefinition.Name[..^2];
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
            var suffixNameRun = Run(typeName, Styles.TokenListBrush);
            return new SingleRunInline(suffixNameRun);
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

        var typeNameRun = Run(typeName, Styles.ClassMainBrush);
        return new SingleRunInline(typeNameRun);
    }

    private static Run Run(string text, IBrush brush)
    {
        return new(text) { Foreground = brush };
    }

    private static DisplayValueSource Property(string name)
    {
        return new(DisplayValueSource.SymbolKind.Property, name);
    }
}

partial class SyntaxViewNodeLineCreator
{
    public sealed class SyntaxTreeViewNodeObjectSpanComparer : IComparer<AnalysisTreeListNode>
    {
        public static SyntaxTreeViewNodeObjectSpanComparer Instance { get; } = new();

        public int Compare(AnalysisTreeListNode? x, AnalysisTreeListNode? y)
        {
            ArgumentNullException.ThrowIfNull(x, nameof(x));
            ArgumentNullException.ThrowIfNull(y, nameof(y));

            var xObject = x.AssociatedSyntaxObject!.Span;
            var yObject = y.AssociatedSyntaxObject!.Span;
            return xObject.CompareTo(yObject);
        }
    }
}

partial class SyntaxViewNodeLineCreator
{
    public static class Types
    {
        public const string Node = "N";
        public const string SyntaxList = "SL";
        public const string Token = "T";
        public const string TokenList = "TL";
        public const string DisplayValue = "D";

        // this is empty for now
        public const string PropertyAnalysisValue = "";

        public const string WhitespaceTrivia = "_";
        public const string CommentTrivia = "/*";
        public const string DirectiveTrivia = "#";
        public const string DisabledTextTrivia = "~";
        public const string EndOfLineTrivia = @"\n";
    }

    public static class Styles
    {
        public static readonly Color ClassMainColor = Color.FromUInt32(0xFF33E5A5);
        public static readonly Color ClassSecondaryColor = Color.FromUInt32(0xFF008052);
        public static readonly Color PropertyColor = Color.FromUInt32(0xFFE5986C);
        public static readonly Color MethodColor = Color.FromUInt32(0xFFFFF4B9);
        public static readonly Color SplitterColor = Color.FromUInt32(0xFFB4B4B4);
        public static readonly Color KeywordColor = Color.FromUInt32(0xFF38A0FF);
        public static readonly Color WhitespaceTriviaColor = Color.FromUInt32(0xFFB3B3B3);
        public static readonly Color WhitespaceTriviaKindColor = Color.FromUInt32(0xFF808080);
        public static readonly Color TokenKindColor = Color.FromUInt32(0xFF7A68E5);
        public static readonly Color FadeTokenKindColor = Color.FromUInt32(0xFF514599);
        public static readonly Color TokenListColor = Color.FromUInt32(0xFF74A3FF);
        public static readonly Color SyntaxListColor = Color.FromUInt32(0xFF79BCA4);
        public static readonly Color EofColor = Color.FromUInt32(0xFF76788B);
        public static readonly Color RawValueColor = Color.FromUInt32(0xFFE4E4E4);
        public static readonly Color WhitespaceTriviaNodeTypeColor = Color.FromUInt32(0xFF7C7C7C);
        public static readonly Color DisplayValueNodeTypeColor = Color.FromUInt32(0xFFCC935F);
        public static readonly Color CommentTriviaNodeTypeColor = Color.FromUInt32(0xFF00A858);
        public static readonly Color CommentTriviaContentColor = Color.FromUInt32(0xFF00703A);
        public static readonly Color CommentTriviaTokenKindColor = Color.FromUInt32(0xFF004D28);
        public static readonly Color DisabledTextTriviaNodeTypeColor = Color.FromUInt32(0xFF8B4D4D);
        public static readonly Color DisabledTextTriviaContentColor = Color.FromUInt32(0xFF664747);
        public static readonly Color DisabledTextTriviaTokenKindColor = Color.FromUInt32(0xFF4D3636);
        public static readonly Color MissingTokenIndicatorColor = Color.FromUInt32(0xFF8B4D4D);

        public static readonly SolidColorBrush ClassMainBrush = new(ClassMainColor);
        public static readonly SolidColorBrush ClassSecondaryBrush = new(ClassSecondaryColor);
        public static readonly SolidColorBrush PropertyBrush = new(PropertyColor);
        public static readonly SolidColorBrush MethodBrush = new(MethodColor);
        public static readonly SolidColorBrush SplitterBrush = new(SplitterColor);
        public static readonly SolidColorBrush KeywordBrush = new(KeywordColor);
        public static readonly SolidColorBrush WhitespaceTriviaBrush = new(WhitespaceTriviaColor);
        public static readonly SolidColorBrush WhitespaceTriviaKindBrush = new(WhitespaceTriviaKindColor);
        public static readonly SolidColorBrush TokenKindBrush = new(TokenKindColor);
        public static readonly SolidColorBrush FadeTokenKindBrush = new(FadeTokenKindColor);
        public static readonly SolidColorBrush TokenListBrush = new(TokenListColor);
        public static readonly SolidColorBrush SyntaxListBrush = new(SyntaxListColor);
        public static readonly SolidColorBrush EofBrush = new(EofColor);
        public static readonly SolidColorBrush RawValueBrush = new(RawValueColor);
        public static readonly SolidColorBrush WhitespaceTriviaNodeTypeBrush = new(WhitespaceTriviaNodeTypeColor);
        public static readonly SolidColorBrush DisplayValueNodeTypeBrush = new(DisplayValueNodeTypeColor);
        public static readonly SolidColorBrush CommentTriviaNodeTypeBrush = new(CommentTriviaNodeTypeColor);
        public static readonly SolidColorBrush CommentTriviaContentBrush = new(CommentTriviaContentColor);
        public static readonly SolidColorBrush CommentTriviaTokenKindBrush = new(CommentTriviaTokenKindColor);
        public static readonly SolidColorBrush DisabledTextTriviaNodeTypeBrush = new(DisabledTextTriviaNodeTypeColor);
        public static readonly SolidColorBrush DisabledTextTriviaContentBrush = new(DisabledTextTriviaContentColor);
        public static readonly SolidColorBrush DisabledTextTriviaTokenKindBrush = new(DisabledTextTriviaTokenKindColor);
        public static readonly SolidColorBrush MissingTokenIndicatorBrush = new(MissingTokenIndicatorColor);

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

        public static readonly NodeTypeDisplay PropertyAnalysisValueDisplay
            = new(Types.PropertyAnalysisValue, DisplayValueNodeTypeColor);

        public static readonly NodeTypeDisplay WhitespaceTriviaDisplay
            = new(Types.WhitespaceTrivia, WhitespaceTriviaNodeTypeColor);
        public static readonly NodeTypeDisplay DirectiveTriviaDisplay
            = new(Types.DirectiveTrivia, WhitespaceTriviaNodeTypeColor);
        public static readonly NodeTypeDisplay EndOfLineTriviaDisplay
            = new(Types.EndOfLineTrivia, WhitespaceTriviaNodeTypeColor);
        public static readonly NodeTypeDisplay CommentTriviaDisplay
            = new(Types.CommentTrivia, CommentTriviaNodeTypeColor);
        public static readonly NodeTypeDisplay DisabledTextTriviaDisplay
            = new(Types.DisabledTextTrivia, DisabledTextTriviaNodeTypeColor);
    }
}