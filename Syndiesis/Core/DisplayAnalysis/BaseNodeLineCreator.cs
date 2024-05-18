using Avalonia.Controls.Documents;
using Avalonia.Media;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System.Text;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract partial class BaseNodeLineCreator(NodeLineCreationOptions options)
{
    protected readonly NodeLineCreationOptions Options = options;

    public abstract AnalysisTreeListNode? CreateRootViewNode(
        object? value, DisplayValueSource valueSource = default);

    protected void AppendValueSource(
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

    protected void AppendMethodDetail(
        DisplayValueSource valueSource, GroupedRunInlineCollection inlines)
    {
        var propertyNameRun = Run(valueSource.Name!, CommonStyles.MethodBrush);
        var parenthesesRun = Run("()", CommonStyles.RawValueBrush);
        var frontGroup = new SimpleGroupedRunInline([
            propertyNameRun,
            parenthesesRun,
        ]);

        if (valueSource.IsAsync)
        {
            var awaitRun = CreateAwaitRun();
            frontGroup.Children.Insert(0, awaitRun);
        }

        var colonRun = Run(":  ", CommonStyles.SplitterBrush);
        inlines.AddRange([
            frontGroup,
            colonRun
        ]);
    }

    protected void AppendPropertyDetail(string propertyName, GroupedRunInlineCollection inlines)
    {
        var propertyNameRun = Run(propertyName, CommonStyles.PropertyBrush);
        var colonRun = Run(":  ", CommonStyles.SplitterBrush);
        inlines.AddSingle(propertyNameRun);
        inlines.Add(colonRun);
    }

    protected AnalysisTreeListNode CreateNodeForSimplePropertyValue(
        object? value,
        string? propertyName = null)
    {
        DisplayValueSource valueSource = default;
        if (propertyName is not null)
        {
            valueSource = Property(propertyName);
        }
        return CreateNodeForSimpleValue(value, valueSource);
    }

    protected AnalysisTreeListNode CreateNodeForSimpleValue(
        object? value,
        DisplayValueSource valueSource = default)
    {
        var line = LineForNodeValue(value, valueSource);
        return new()
        {
            NodeLine = line,
        };
    }

    protected AnalysisTreeListNodeLine LineForNodeValue(
        object? value, DisplayValueSource valueSource = default)
    {
        var inlines = new GroupedRunInlineCollection();

        AppendValueSource(valueSource, inlines);
        var valueRun = RunForSimpleObjectValue(value);
        inlines.Add(valueRun);

        return new()
        {
            GroupedRunInlines = inlines,
            NodeTypeDisplay = CommonStyles.PropertyAnalysisValueDisplay,
        };
    }

    protected SingleRunInline RunForSimpleObjectValue(object? value)
    {
        if (value is null)
            return new SingleRunInline(CreateNullValueRun());

        var fullValue = value;

        if (value is string stringValue)
        {
            // truncate the string
            value = SimplifyWhitespace(stringValue);
        }

        return RunForSimpleObjectValue(value, fullValue);
    }

    protected SingleRunInline RunForSimpleObjectValue(object value, object fullValue)
    {
        var text = value.ToString()!;
        var fullText = fullValue!.ToString()!;
        var run = Run(text, CommonStyles.RawValueBrush);
        return new SingleRunInline(run, fullText);
    }

    protected static Run CreateAwaitRun()
    {
        return Run("await ", CommonStyles.KeywordBrush);
    }

    protected static Run Run(string text, IBrush brush)
    {
        return new(text) { Foreground = brush };
    }

    protected static DisplayValueSource Property(string name)
    {
        return new(DisplayValueSource.SymbolKind.Property, name);
    }

    protected static Run CreateNullValueRun()
    {
        const string nullDisplay = "[null]";
        var run = Run(nullDisplay, CommonStyles.NullValueBrush);
        run.FontStyle = FontStyle.Italic;
        return run;
    }

    protected string SimplifyWhitespace(string source)
    {
        return SimplifyWhitespace(source, Options.TruncationLimit);
    }

    protected static string SimplifyWhitespace(string source, int truncationLength)
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

    protected static string CreateDisplayStringForEndOfLineText(string text)
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

    protected static Run NewValueKindSplitterRun()
    {
        return Run("      ", CommonStyles.RawValueBrush);
    }
}

partial class BaseNodeLineCreator
{
    public abstract class CommonTypes
    {
        public const string PropertyAnalysisValue = "";
    }

    public abstract class CommonStyles
    {
        public static readonly Color RawValueColor = Color.FromUInt32(0xFFE4E4E4);
        public static readonly Color KeywordColor = Color.FromUInt32(0xFF38A0FF);
        public static readonly Color NullValueColor = Color.FromUInt32(0xFF808080);
        public static readonly Color PropertyColor = Color.FromUInt32(0xFFE5986C);
        public static readonly Color MethodColor = Color.FromUInt32(0xFFFFF4B9);
        public static readonly Color SplitterColor = Color.FromUInt32(0xFFB4B4B4);

        public static readonly SolidColorBrush RawValueBrush = new(RawValueColor);
        public static readonly SolidColorBrush KeywordBrush = new(KeywordColor);
        public static readonly SolidColorBrush NullValueBrush = new(NullValueColor);
        public static readonly SolidColorBrush PropertyBrush = new(PropertyColor);
        public static readonly SolidColorBrush MethodBrush = new(MethodColor);
        public static readonly SolidColorBrush SplitterBrush = new(SplitterColor);

        public static readonly NodeTypeDisplay PropertyAnalysisValueDisplay
            = new(CommonTypes.PropertyAnalysisValue, RawValueColor);
    }
}
