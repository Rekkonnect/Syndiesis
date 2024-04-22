using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSharpSyntaxEditor.Controls.SyntaxVisualization.Creation;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed class NodeLineCreationOptions
{
    public bool ShowTrivia = true;

    [Obsolete("Not yet implemented")]
    public bool ShowOperations = false;

    public bool OmitCompilationUnitRoot = false;
}

public sealed partial class NodeLineCreator(NodeLineCreationOptions options)
{
    private readonly NodeLineCreationOptions _options = options;

    public SyntaxTreeListNode CreateRootNodeOrToken(
        SyntaxNodeOrToken nodeOrToken, string? propertyName = null)
    {
        var rootLine = CreateNodeOrTokenLine(nodeOrToken, propertyName);
        var children = CreateNodeOrTokenChildren(nodeOrToken);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildNodes = new(children),
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
        var children = CreateNodeChildren(node);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildNodes = new(children),
            AssociatedSyntaxObjectContent = node,
        };
    }

    public SyntaxTreeListNode CreateTokenNode(SyntaxToken token, string? propertyName = null)
    {
        var rootLine = CreateTokenNodeLine(token, propertyName);
        var children = CreateTokenChildren(token);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildNodes = new(children),
            AssociatedSyntaxObjectContent = token,
        };
    }

    public SyntaxTreeListNode CreateRootNode(ReadOnlySyntaxNodeList node, string? propertyName = null)
    {
        var rootLine = CreateSyntaxListLine(node, propertyName);
        var children = CreateNodeListChildren(node);
        return new SyntaxTreeListNode
        {
            NodeLine = rootLine,
            ChildNodes = new(children),
            AssociatedSyntaxObjectContent = node,
        };
    }

    private IReadOnlyList<SyntaxTreeListNode> CreateTokenChildren(SyntaxToken token)
    {
        if (token.Kind() is SyntaxKind.EndOfFileToken)
            return [];

        var leadingTrivia = token.LeadingTrivia;
        var trailingTrivia = token.TrailingTrivia;
        int triviaCount = leadingTrivia.Count + trailingTrivia.Count;
        if (!_options.ShowTrivia)
        {
            triviaCount = 0;
        }
        var children = new List<SyntaxTreeListNode>(triviaCount + 1);

        AppendTriviaList(leadingTrivia, children);

        var displayNode = CreateDisplayNode(token);
        children.Add(displayNode);

        AppendTriviaList(trailingTrivia, children);

        // sort?
        children.Sort(SyntaxTreeViewNodeObjectSpanComparer.Instance);

        return children;
    }

    private SyntaxTreeListNode CreateDisplayNode(SyntaxToken token)
    {
        return new()
        {
            NodeLine = CreateDisplayNodeLine(token),
            AssociatedSyntaxObjectContent = token,
        };
    }

    private SyntaxTreeListNodeLine CreateDisplayNodeLine(SyntaxToken token)
    {
        var text = token.ValueText;
        var inlines = new InlineCollection
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

                // TODO: Rest
            }
        }

        children.Sort(SyntaxTreeViewNodeObjectSpanComparer.Instance);

        return children;
    }

    private SyntaxTreeListNode CreateTokenListNode(SyntaxTokenList list, string? propertyName)
    {
        var node = CreateTokenListNodeLine(list, propertyName);
        var children = CreateTokenListChildren(list);

        return new()
        {
            NodeLine = node,
            ChildNodes = new(children),
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
        // we don't like infinite recursion
        if (propertyInfo.Name is nameof(SyntaxNode.Parent))
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
        return new()
        {
            NodeLine = line,
            AssociatedSyntaxObjectContent = trivia,
        };
    }

    public SyntaxTreeListNodeLine CreateTriviaLine(SyntaxTrivia trivia)
    {
        var inlines = new InlineCollection();

        var displayText = DisplayTextForTrivia(trivia);
        var displayTextRun = Run(displayText, Styles.TriviaBrush);
        inlines.Add(displayTextRun);

        inlines.Add(NewValueKindSplitterRun());

        var triviaKindText = trivia.Kind().ToString();
        var triviaKindRun = Run(triviaKindText, Styles.TriviaKindBrush);
        triviaKindRun.FontStyle = FontStyle.Italic;
        inlines.Add(triviaKindRun);

        return new()
        {
            Inlines = inlines,
            NodeTypeDisplay = Styles.TriviaDisplay,
        };
    }

    private static string DisplayTextForTrivia(SyntaxTrivia trivia)
    {
        var structure = trivia.GetStructure();
        if (structure is null)
        {
            return DisplayTextForUnstructuredTrivia(trivia);
        }
        // TODO: handle structured trivia
        return null;
    }

    private static string DisplayTextForUnstructuredTrivia(SyntaxTrivia trivia)
    {
        var kind = trivia.Kind();
        switch (kind)
        {
            case SyntaxKind.WhitespaceTrivia:
                return WhitespaceTriviaText(trivia);
            case SyntaxKind.EndOfLineTrivia:
                return EndOfLineTriviaText(trivia);
        }

        throw new ArgumentException("Invalid trivia syntax kind; expected whitespace or EOL trivia");
    }

    private static string EndOfLineTriviaText(SyntaxTrivia trivia)
    {
        var text = trivia.ToFullString();
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
            var iteratedWhitespace = GetWhitespaceKind(c);
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

    private static WhitespaceKind GetWhitespaceKind(char c)
    {
        return c switch
        {
            ' ' => WhitespaceKind.Space,
            '\t' => WhitespaceKind.Tab,
            _ => WhitespaceKind.None,
        };
    }

    private void AppendTokenKindDetails(SyntaxToken token, string? propertyName, InlineCollection inlines)
    {
        var kind = token.Kind();
        var kindName = kind.ToString();
        bool hasEqualName = propertyName == kindName;
        bool isKeyword = SyntaxFacts.IsKeywordKind(kind);

        var displayText = token.ToString();
        var displayBrush = isKeyword
            ? Styles.KeywordBrush
            : Styles.RawValueBrush;

        // special case for EOF
        if (kind is SyntaxKind.EndOfFileToken)
        {
            displayBrush = Styles.EofBrush;
            displayText = "[EOF]";
        }
        var displayTextRun = Run(displayText, displayBrush);

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

    private static Run NewValueKindSplitterRun()
    {
        return Run("    ", Styles.RawValueBrush);
    }

    private void AppendPropertyDetail(string? propertyName, InlineCollection inlines)
    {
        if (propertyName is null)
            return;

        var propertyNameRun = Run(propertyName, Styles.PropertyBrush);
        var colonRun = Run(": ", Styles.SplitterBrush);
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

            var xObject = x.AssociatedSyntaxObject;
            var yObject = y.AssociatedSyntaxObject;
            return SyntaxObjectSpanComparer.Instance.Compare(xObject, yObject);
        }
    }
    public sealed class SyntaxObjectSpanComparer : IComparer
    {
        public static SyntaxObjectSpanComparer Instance { get; } = new();

        public int Compare(object? x, object? y)
        {
            var xSpan = FullSpan(x);
            var ySpan = FullSpan(y);
            return xSpan.CompareTo(ySpan);
        }

        private static TextSpan FullSpan(object? x)
        {
            ArgumentNullException.ThrowIfNull(x);

            switch (x)
            {
                case SyntaxNode node:
                    return node.FullSpan;

                case SyntaxToken token:
                    return token.FullSpan;

                case SyntaxTrivia trivia:
                    return trivia.FullSpan;

                case IReadOnlyList<object?> nodeList:
                    var first = nodeList.FirstOrDefault();
                    if (first is null)
                        throw new NullReferenceException();

                    return FullSpan(first);
            }

            return (x as dynamic).FullSpan;
        }
    }
}

public sealed record SyntaxObjectInfo(
    object SyntaxObject, TextSpan Span, TextSpan FullSpan)
{
    public static SyntaxObjectInfo? GetInfoForObject(object? x)
    {
        if (x is null)
            return null;

        if (x is SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                var node = nodeOrToken.AsNode()!;
                return GetInfoForObject(node);
            }
            else
            {
                var token = nodeOrToken.AsToken()!;
                return GetInfoForObject(token);
            }
        }

        var span = GetSpan(x);
        var fullSpan = GetFullSpan(x);
        return new(x, span, fullSpan);
    }

    private static TextSpan GetSpan(object x)
    {
        switch (x)
        {
            case SyntaxNode node:
                return node.Span;

            case SyntaxToken token:
                return token.Span;

            case SyntaxTrivia trivia:
                return trivia.Span;

            case IReadOnlyList<object?> nodeList:
            {
                return ExtractSpanFromList(nodeList, GetSpan);
            }
        }

        return (x as dynamic).Span;
    }

    private static TextSpan GetFullSpan(object x)
    {
        switch (x)
        {
            case SyntaxNode node:
                return node.FullSpan;

            case SyntaxToken token:
                return token.FullSpan;

            case SyntaxTrivia trivia:
                return trivia.FullSpan;

            case IReadOnlyList<object?> nodeList:
            {
                return ExtractSpanFromList(nodeList, GetFullSpan);
            }
        }

        return (x as dynamic).FullSpan;
    }

    private static TextSpan ExtractSpanFromList(
        IReadOnlyList<object?> nodeList,
        Func<object, TextSpan> spanGetter)
    {
        if (nodeList.Count is 0)
            throw new ArgumentException("Invalid empty list provided");

        var first = nodeList[0];
        var firstSpan = spanGetter(first!);
        if (nodeList.Count is 1)
            return firstSpan;

        var start = firstSpan.Start;
        var last = nodeList[^1];
        var lastSpan = spanGetter(last!);
        var end = lastSpan.End;
        return new TextSpan(start, end);
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
        public const string Trivia = "V";
    }

    public static class Styles
    {
        public static readonly Color ClassMainColor = Color.FromUInt32(0xFF33E5A5);
        public static readonly Color ClassSecondaryColor = Color.FromUInt32(0xFF008052);
        public static readonly Color PropertyColor = Color.FromUInt32(0xFFE5986C);
        public static readonly Color SplitterColor = Color.FromUInt32(0xFFB4B4B4);
        public static readonly Color KeywordColor = Color.FromUInt32(0xFF38A0FF);
        public static readonly Color TriviaColor = Color.FromUInt32(0xFFB3B3B3);
        public static readonly Color TriviaKindColor = Color.FromUInt32(0xFF808080);
        public static readonly Color TokenKindColor = Color.FromUInt32(0xFF7A68E5);
        public static readonly Color FadeTokenKindColor = Color.FromUInt32(0xFF443A80);
        public static readonly Color TokenListColor = Color.FromUInt32(0xFF74A3FF);
        public static readonly Color SyntaxListColor = Color.FromUInt32(0xFF79BCA4);
        public static readonly Color EofColor = Color.FromUInt32(0xFF76788B);
        public static readonly Color RawValueColor = Color.FromUInt32(0xFFE4E4E4);
        public static readonly Color TriviaNodeTypeColor = Color.FromUInt32(0xFF7C7C7C);
        public static readonly Color DisplayValueNodeTypeColor = Color.FromUInt32(0xFFCC935F);

        public static readonly SolidColorBrush ClassMainBrush = new(ClassMainColor);
        public static readonly SolidColorBrush ClassSecondaryBrush = new(ClassSecondaryColor);
        public static readonly SolidColorBrush PropertyBrush = new(PropertyColor);
        public static readonly SolidColorBrush SplitterBrush = new(SplitterColor);
        public static readonly SolidColorBrush KeywordBrush = new(KeywordColor);
        public static readonly SolidColorBrush TriviaBrush = new(TriviaColor);
        public static readonly SolidColorBrush TriviaKindBrush = new(TriviaKindColor);
        public static readonly SolidColorBrush TokenKindBrush = new(TokenKindColor);
        public static readonly SolidColorBrush FadeTokenKindBrush = new(FadeTokenKindColor);
        public static readonly SolidColorBrush TokenListBrush = new(TokenListColor);
        public static readonly SolidColorBrush SyntaxListBrush = new(SyntaxListColor);
        public static readonly SolidColorBrush EofBrush = new(EofColor);
        public static readonly SolidColorBrush RawValueBrush = new(RawValueColor);
        public static readonly SolidColorBrush TriviaNodeTypeBrush = new(TriviaNodeTypeColor);
        public static readonly SolidColorBrush DisplayValueNodeTypeBrush = new(DisplayValueNodeTypeColor);

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
        public static readonly NodeTypeDisplay TriviaDisplay
            = new(Types.Trivia, TriviaNodeTypeColor);
    }
}
