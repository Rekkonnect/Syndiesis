using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Syndiesis.Controls.SyntaxVisualization.Creation;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed class NodeLineCreationOptions
{
    public bool ShowTrivia = true;

    public int TruncationLimit = 30;
}

public sealed partial class NodeLineCreator(NodeLineCreationOptions options)
{
    private const string eofDisplayString = "[EOF]";
    private const string missingTokenDisplayString = "[Missing]";

    private readonly NodeLineCreationOptions _options = options;

    public SyntaxTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, string? propertyName = null)
    {
        var rootLine = CreateNodeOrTokenLine(nodeOrToken, propertyName);
        var children = () => CreateNodeOrTokenChildren(nodeOrToken);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = nodeOrToken,
        };
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateNodeOrTokenChildren(SyntaxNodeOrToken nodeOrToken)
    {
        if (nodeOrToken.IsNode)
        {
            return CreateNodeChildren(nodeOrToken.AsNode()!);
        }

        return CreateTokenChildren(nodeOrToken.AsToken());
    }

    private SyntaxTreeListNodeLine CreateNodeOrTokenLine(
        SyntaxNodeOrToken nodeOrToken, string? propertyName)
    {
        if (nodeOrToken.IsNode)
        {
            return CreateNodeLine(nodeOrToken.AsNode()!, propertyName);
        }

        return CreateTokenNodeLine(nodeOrToken.AsToken(), propertyName);
    }

    public SyntaxTreeListNode CreateRootNode(SyntaxNode node, string? propertyName = null)
    {
        var rootLine = CreateNodeLine(node, propertyName);
        var children = () => CreateNodeChildren(node);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = node,
        };
    }

    public SyntaxTreeListNode CreateTokenNode(SyntaxToken token, string? propertyName = null)
    {
        var rootLine = CreateTokenNodeLine(token, propertyName);
        var children = GetChildRetrieverForToken(token);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = token,
        };
    }

    private Func<IReadOnlyList<SyntaxTreeListNode>>? GetChildRetrieverForToken(SyntaxToken token)
    {
        switch (token.Kind())
        {
            case SyntaxKind.XmlTextLiteralNewLineToken:
                return null;
        }

        return () => CreateTokenChildren(token);
    }

    public SyntaxTreeListNode CreateRootNode(ReadOnlySyntaxNodeList node, string? propertyName = null)
    {
        var rootLine = CreateSyntaxListLine(node, propertyName);
        var children = () => CreateNodeListChildren(node);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = node,
        };
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateTokenChildren(SyntaxToken token)
    {
        int triviaCount = 0;
        SyntaxTriviaList leadingTrivia = default;
        SyntaxTriviaList trailingTrivia = default;
        if (_options.ShowTrivia)
        {
            leadingTrivia = token.LeadingTrivia;
            trailingTrivia = token.TrailingTrivia;
            triviaCount = leadingTrivia.Count + trailingTrivia.Count;
        }
        var children = new List<SyntaxTreeListNode>(triviaCount + 1);

        // they will be sorted anyway
        if (_options.ShowTrivia)
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

    private SyntaxTreeListNode CreateEndOfFileDisplayNode(SyntaxToken token)
    {
        return new()
        {
            NodeLine = CreateEndOfFileDisplayNodeLine(),
            AssociatedSyntaxObjectContent = token,
        };
    }

    private SyntaxTreeListNodeLine CreateEndOfFileDisplayNodeLine()
    {
        var eofRun = CreateEofRun();

        return new()
        {
            Inlines = [eofRun],
            NodeTypeDisplay = Styles.DisplayValueDisplay,
        };
    }

    private static Run CreateEofRun()
    {
        return Run(eofDisplayString, Styles.EofBrush);
    }

    private SyntaxTreeListNode CreateDisplayNode(SyntaxToken token)
    {
        return new()
        {
            NodeLine = CreateDisplayNodeLine(token),
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

    private SyntaxTreeListNodeLine CreateDisplayNodeLine(SyntaxToken token)
    {
        var text = token.ValueText;

        if (IsStringLiteralKind(token.Kind()))
        {
            text = SimplifyWhitespace(text);
        }

        var inlines = new InlineCollection()
        {
            Run(text, Styles.RawValueBrush),
        };

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.DisplayValueDisplay,
        };
    }

    private void AppendTriviaList(SyntaxTriviaList leadingTrivia, List<SyntaxTreeListNode> children)
    {
        if (_options.ShowTrivia)
        {
            foreach (var trivia in leadingTrivia)
            {
                var triviaNode = CreateTriviaNode(trivia);
                children.Add(triviaNode);
            }
        }
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateNodeChildren(SyntaxNode node)
    {
        var properties = GetInterestingPropertiesForNodeType(node);

        var children = new List<SyntaxTreeListNode>(properties.Count);

        foreach (var property in properties)
        {
            var value = property.GetValue(node);
            bool includeValue = ShouldIncludeValue(value);
            if (!includeValue)
            {
                continue;
            }

            switch (value)
            {
                case SyntaxNode childNode:
                    var childNodeElement = CreateRootNode(childNode, property.Name);
                    children.Add(childNodeElement);
                    break;

                case ReadOnlySyntaxNodeList childNodeList:
                    var childNodeListElement = CreateRootNode(childNodeList, property.Name);
                    children.Add(childNodeListElement);
                    break;

                case SyntaxToken token:
                    var tokenNode = CreateTokenNode(token, property.Name);
                    children.Add(tokenNode);
                    break;

                case SyntaxTokenList tokenList:
                    var tokenListNode = CreateTokenListNode(tokenList, property.Name);
                    children.Add(tokenListNode);
                    break;
            }
        }

        children.Sort(SyntaxTreeViewNodeObjectSpanComparer.Instance);

        return children;
    }

    private SyntaxTreeListNode CreateTokenListNode(SyntaxTokenList list, string? propertyName)
    {
        var node = CreateTokenListNodeLine(list, propertyName);
        var children = () => CreateTokenListChildren(list);

        return new()
        {
            NodeLine = node,
            ChildRetriever = children,
            AssociatedSyntaxObjectContent = list,
        };
    }

    private SyntaxTreeListNodeLine CreateTokenListNodeLine(SyntaxTokenList list, string? propertyName)
    {
        var inlines = CreateBasicTypeNameInlines(list, propertyName);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.TokenListNodeDisplay,
        };
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateTokenListChildren(SyntaxTokenList list)
    {
        return list
            .Select(s => CreateTokenNode(s))
            .ToList()
            ;
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateNodeListChildren(ReadOnlySyntaxNodeList list)
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

    private IReadOnlyList<SyntaxTreeListNode> CreateNodeListChildren(SyntaxNodeOrTokenList list)
    {
        return list
            .Select(s => CreateRootNodeOrToken(s))
            .ToList()
            ;
    }

    private static IReadOnlyList<PropertyInfo> GetInterestingPropertiesForNodeType(SyntaxNode node)
    {
        var nodeType = node.GetType();
        var properties = nodeType.GetProperties();
        var interestingTypeProperties = properties.Where(FilterNodeProperty)
            .ToArray();

        return interestingTypeProperties;
    }

    private static bool FilterNodeProperty(PropertyInfo propertyInfo)
    {
        var name = propertyInfo.Name;

        // we don't like infinite recursion
        if (name is nameof(SyntaxNode.Parent))
            return false;

        bool extraFilter = IsExtraProperty(propertyInfo, name);
        if (extraFilter)
            return false;

        var type = propertyInfo.PropertyType;

        if (type.IsGenericType)
        {
            var interfaces = type.GetInterfaces();
            bool isListOfSyntaxNodes = interfaces.Any(
                i => i.IsAssignableTo(typeof(ReadOnlySyntaxNodeList)));
            return isListOfSyntaxNodes;
        }

        if (IsSyntaxNodeType(type))
            return true;

        return type == typeof(SyntaxTokenList)
            || type == typeof(SyntaxTriviaList)
            || type == typeof(SyntaxToken)
            || type == typeof(SyntaxTrivia)
            ;
    }

    private static bool IsExtraProperty(PropertyInfo propertyInfo, string name)
    {
        if (name is nameof(UsingDirectiveSyntax.Name))
        {
            if (propertyInfo.DeclaringType == typeof(UsingDirectiveSyntax))
                return true;
        }

        if (name is nameof(DirectiveTriviaSyntax.DirectiveNameToken))
        {
            if (propertyInfo.DeclaringType == typeof(DirectiveTriviaSyntax))
                return true;
        }

        return false;
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
                if (!_options.ShowTrivia)
                    return false;

                if (triviaList == default)
                    return false;

                return triviaList.Count > 0;

            case SyntaxToken token:
                if (token.IsMissing)
                    return false;

                return token != default;

            case SyntaxTrivia trivia:
                if (!_options.ShowTrivia)
                    return false;

                return trivia != default;

            default:
                return false;
        }
    }

    private static bool IsSyntaxNodeType(Type type)
    {
        var current = type;
        while (true)
        {
            if (current is null)
                return false;

            if (current == typeof(SyntaxNode))
                return true;

            current = current.BaseType;
        }
    }

    public SyntaxTreeListNodeLine CreateNodeLine(SyntaxNode node, string? propertyName = null)
    {
        var inlines = CreateBasicTypeNameInlines(node, propertyName);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.ClassNodeDisplay,
        };
    }

    public SyntaxTreeListNodeLine CreateSyntaxListLine(
        ReadOnlySyntaxNodeList list, string? propertyName = null)
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
                return CreateBasicSyntaxListLine(list, propertyName);
            }

            var typeArgument = listType.GenericTypeArguments[0];

            // keep this until it's hopefully not used
            if (genericDefinition == typeof(SyntaxList<>))
            {
                const string methodName = nameof(CreateSyntaxListLine);
                var invokedMethod = this.GetType()
                    .GetMethod(nameof(methodName))
                    !.MakeGenericMethod([typeArgument]);
                var result = invokedMethod.Invoke(this, [list, propertyName])
                    as SyntaxTreeListNodeLine;
                return result!;
            }
            if (genericDefinition == typeof(SeparatedSyntaxList<>))
            {
                const string methodName = nameof(CreateSeparatedSyntaxListLine);
                var invokedMethod = this.GetType()
                    .GetMethod(nameof(methodName))
                    !.MakeGenericMethod([typeArgument]);
                var result = invokedMethod.Invoke(this, [list, propertyName])
                    as SyntaxTreeListNodeLine;
                return result!;
            }
        }

        throw new ArgumentException("Invalid syntax node list type");
    }

    public SyntaxTreeListNodeLine CreateBasicSyntaxListLine(
        ReadOnlySyntaxNodeList node, string? propertyName = null)
    {
        var inlines = CreateBasicTypeNameInlines(node, propertyName);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
    }

    public SyntaxTreeListNodeLine CreateSyntaxListLine<T>(
        SyntaxList<T> node, string? propertyName = null)
        where T : SyntaxNode
    {
        var inlines = CreateBasicTypeNameInlines(node, propertyName);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
    }

    public SyntaxTreeListNodeLine CreateSeparatedSyntaxListLine<T>(
        SeparatedSyntaxList<T> node, string? propertyName = null)
        where T : SyntaxNode
    {
        var inlines = CreateBasicTypeNameInlines(node, propertyName);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.SyntaxListNodeDisplay,
        };
    }

    private InlineCollection CreateBasicTypeNameInlines(object value, string? propertyName)
    {
        var inlines = new InlineCollection();
        var type = value.GetType();

        AppendPropertyDetail(propertyName, inlines);
        AppendTypeDetails(type, inlines);

        return inlines;
    }

    public SyntaxTreeListNodeLine CreateTokenNodeLine(SyntaxToken token, string? propertyName)
    {
        var inlines = new InlineCollection();

        AppendPropertyDetail(propertyName, inlines);
        AppendTokenKindDetails(token, propertyName, inlines);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.TokenNodeDisplay,
        };
    }

    public SyntaxTreeListNode CreateTriviaNode(SyntaxTrivia trivia)
    {
        var line = CreateTriviaLine(trivia);
        var structure = trivia.GetStructure();
        Func<IReadOnlyList<SyntaxTreeListNode>>? children = null;
        if (structure is not null)
        {
            var structureNode = CreateRootNode(structure, "Structure");
            children = () => [structureNode];
        }

        return new()
        {
            NodeLine = line,
            AssociatedSyntaxObjectContent = trivia,
            ChildRetriever = children,
        };
    }

    public SyntaxTreeListNodeLine CreateTriviaLine(SyntaxTrivia trivia)
    {
        var inlines = new InlineCollection();

        var display = FormatTriviaDisplay(trivia, inlines);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = display,
        };
    }

    private NodeTypeDisplay FormatTriviaDisplay(SyntaxTrivia trivia, InlineCollection inlines)
    {
        var structure = trivia.GetStructure();
        if (structure is null)
        {
            return FormatUnstructuredTriviaDisplay(trivia, inlines);
        }

        return FormatStructuredTriviaDisplay(trivia, inlines);
    }

    private NodeTypeDisplay FormatStructuredTriviaDisplay(
        SyntaxTrivia trivia, InlineCollection inlines)
    {
        var kind = trivia.Kind();
        bool isDirective = SyntaxFacts.IsPreprocessorDirective(kind);
        if (isDirective)
        {
            var displayText = CommentTriviaText(trivia);
            var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
            inlines.Add(displayTextRun);

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
                inlines.Add(displayTextRun);

                return Styles.CommentTriviaDisplay;
            }

            case SyntaxKind.SkippedTokensTrivia:
            {
                var displayText = trivia.Kind().ToString();
                var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                inlines.Add(displayTextRun);

                return Styles.WhitespaceTriviaDisplay;
            }
        }

        Debug.Assert(false, "Unreachable by unknown trivia display");
        return Styles.WhitespaceTriviaDisplay;
    }

    private NodeTypeDisplay FormatUnstructuredTriviaDisplay(
        SyntaxTrivia trivia, InlineCollection inlines)
    {
        var kind = trivia.Kind();
        switch (kind)
        {
            case SyntaxKind.WhitespaceTrivia:
            {
                var displayText = WhitespaceTriviaText(trivia);
                var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                inlines.Add(displayTextRun);

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
                var displayText = CommentTriviaText(trivia);
                var displayTextRun = Run(displayText, Styles.CommentTriviaContentBrush);
                inlines.Add(displayTextRun);

                AddTriviaKindWithSplitter(
                    trivia,
                    Styles.CommentTriviaTokenKindBrush,
                    inlines);

                return Styles.CommentTriviaDisplay;
            }
            case SyntaxKind.BadDirectiveTrivia:
            {
                var displayText = CommentTriviaText(trivia);
                var displayTextRun = Run(displayText, Styles.WhitespaceTriviaBrush);
                inlines.Add(displayTextRun);

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
                inlines.Add(disabledTextRun);

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
                inlines.Add(displayTextRun);

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
        InlineCollection inlines)
    {
        inlines.Add(NewValueKindSplitterRun());

        var triviaKindText = trivia.Kind().ToString();
        var triviaKindRun = Run(triviaKindText, brush);
        triviaKindRun.FontStyle = FontStyle.Italic;
        inlines.Add(triviaKindRun);
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

    private string CommentTriviaText(SyntaxTrivia trivia)
    {
        var text = trivia.ToFullString();
        return SimplifyWhitespace(text);
    }

    private string SimplifyWhitespace(string source)
    {
        return SimplifyWhitespace(source, _options.TruncationLimit);
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
            if (trimmedText.Length == truncationLength + truncationSuffix.Length)
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
        int startLine = lineSpan.Start.Line;
        int endLine = lineSpan.End.Line;

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

    private void AppendTokenKindDetails(SyntaxToken token, string? propertyName, InlineCollection inlines)
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

        inlines.AddRange([
            displayTextRun,
            NewValueKindSplitterRun(),
            kindRun,
        ]);
    }

    private Run CreateDisplayTextRun(SyntaxToken token, SyntaxKind kind, bool isKeyword)
    {
        if (kind is SyntaxKind.EndOfFileToken)
        {
            return CreateEofRun();
        }

        if (kind is SyntaxKind.XmlTextLiteralNewLineToken)
        {
            var eolText = CreateDisplayStringForEndOfLineText(token.ToFullString());
            return Run(eolText, Styles.RawValueBrush);
        }

        if (token.IsMissing)
        {
            return CreateMissingTokenRun();
        }

        var displayText = token.ToString();
        var displayBrush = isKeyword
            ? Styles.KeywordBrush
            : Styles.RawValueBrush;

        if (IsStringLiteralKind(kind))
        {
            displayText = SimplifyWhitespace(displayText);
        }

        return Run(displayText, displayBrush);
    }

    private static Run CreateMissingTokenRun()
    {
        return Run(missingTokenDisplayString, Styles.MissingTokenIndicatorBrush);
    }

    private static Run NewValueKindSplitterRun()
    {
        return Run("    ", Styles.RawValueBrush);
    }

    private void AppendPropertyDetail(string? propertyName, InlineCollection inlines)
    {
        if (propertyName is null)
            return;

        var propertyNameRun = Run(propertyName, Styles.PropertyBrush);
        var colonRun = Run(":  ", Styles.SplitterBrush);
        inlines.Add(propertyNameRun);
        inlines.Add(colonRun);
    }

    private void AppendTypeDetails(Type type, InlineCollection inlines)
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

                inlines.Add(outerRun);
                var argument = type.GenericTypeArguments[0];
                AppendTypeDetails(argument, inlines);
                inlines.Add(closingTag);
            }

            return;
        }

        var typeName = type.Name;

        if (typeName is nameof(SyntaxTokenList))
        {
            var suffixNameRun = Run(typeName, Styles.TokenListBrush);

            inlines.Add(suffixNameRun);
            return;
        }

        const string syntaxSuffix = "Syntax";
        if (typeName.EndsWith(syntaxSuffix))
        {
            var primaryClassNameRun = Run(typeName[..^syntaxSuffix.Length], Styles.ClassMainBrush);
            var suffixNameRun = Run(syntaxSuffix, Styles.ClassSecondaryBrush);

            inlines.Add(primaryClassNameRun);
            inlines.Add(suffixNameRun);
            return;
        }

        var typeNameRun = Run(typeName, Styles.ClassMainBrush);
        inlines.Add(typeNameRun);
    }

    private static Run Run(string text, IBrush brush)
    {
        return new(text) { Foreground = brush };
    }
}

partial class NodeLineCreator
{
    public sealed class SyntaxTreeViewNodeObjectSpanComparer : IComparer<SyntaxTreeListNode>
    {
        public static SyntaxTreeViewNodeObjectSpanComparer Instance { get; } = new();

        public int Compare(SyntaxTreeListNode? x, SyntaxTreeListNode? y)
        {
            ArgumentNullException.ThrowIfNull(x, nameof(x));
            ArgumentNullException.ThrowIfNull(y, nameof(y));

            var xObject = x.AssociatedSyntaxObject!.Span;
            var yObject = y.AssociatedSyntaxObject!.Span;
            return xObject.CompareTo(yObject);
        }
    }
}

partial class NodeLineCreator
{
    public static class Types
    {
        public const string Node = "N";
        public const string SyntaxList = "SL";
        public const string Token = "T";
        public const string TokenList = "TL";
        public const string DisplayValue = "D";

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
        public static readonly Color SplitterColor = Color.FromUInt32(0xFFB4B4B4);
        public static readonly Color KeywordColor = Color.FromUInt32(0xFF38A0FF);
        public static readonly Color WhitespaceTriviaColor = Color.FromUInt32(0xFFB3B3B3);
        public static readonly Color WhitespaceTriviaKindColor = Color.FromUInt32(0xFF808080);
        public static readonly Color TokenKindColor = Color.FromUInt32(0xFF7A68E5);
        public static readonly Color FadeTokenKindColor = Color.FromUInt32(0xFF443A80);
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
