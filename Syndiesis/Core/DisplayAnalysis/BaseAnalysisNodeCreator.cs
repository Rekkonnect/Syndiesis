using Avalonia.Controls.Documents;
using Avalonia.Media;
using Garyon.Reflection;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Syndiesis.Core.DisplayAnalysis;

public delegate IReadOnlyList<AnalysisTreeListNode> AnalysisNodeChildRetriever();

public abstract partial class BaseAnalysisNodeCreator
{
    protected readonly AnalysisNodeCreationOptions Options;

    private readonly GeneralRootViewNodeCreator _generalCreator;
    private readonly NullValueRootAnalysisNodeCreator _nullValueCreator;
    private readonly BooleanRootAnalysisNodeCreator _booleanCreator;

    public BaseAnalysisNodeCreator(AnalysisNodeCreationOptions options)
    {
        Options = options;

        _generalCreator = new(this);
        _nullValueCreator = new(this);
        _booleanCreator = new(this);
    }

    public abstract AnalysisTreeListNode? CreateRootViewNode(
        object? value, DisplayValueSource valueSource = default);

    public AnalysisTreeListNode CreateRootGeneral(
        object? value, DisplayValueSource valueSource = default)
    {
        return CreateRootViewNode(value, valueSource)
            ?? CreateRootBasic(value, valueSource);
    }

    public AnalysisTreeListNode CreateRootBasic(
        object? value, DisplayValueSource valueSource = default)
    {
        if (value is null)
            return _nullValueCreator.CreateNode(value, valueSource);

        var type = value.GetType();
        switch (value)
        {
            case bool b:
                return _booleanCreator.CreateNode(b, valueSource);
        }

        if (IsSimpleType(type))
        {

        }

        return _generalCreator.CreateNode(value, valueSource);
    }

    private static bool IsSimpleType(Type type)
    {
        switch (type.GetTypeCode())
        {
            case TypeCode.Empty:
            case TypeCode.Object:
                return false;
        }

        return true;
    }

    private AnalysisTreeListNode CreateRootNullableStruct<T>(
        Nullable<T> value, DisplayValueSource valueSource = default)
        where T : struct
    {
        Debug.Assert(value is not null);

        var inner = value.Value;
        return CreateRootBasic(inner, valueSource);
    }

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
            NodeTypeDisplay = CommonStyles.PropertyAccessValueDisplay,
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

    protected static DisplayValueSource MethodSource(string name)
    {
        return new(DisplayValueSource.SymbolKind.Method, name);
    }

    protected static Run CreateEmptyValueRun()
    {
        const string emptyValueDisplay = "[empty]";
        return Run(emptyValueDisplay, CommonStyles.NullValueBrush);
    }

    protected static SingleRunInline CreateNullValueSingleRun()
    {
        return new(CreateNullValueRun());
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

    protected static GroupedRunInline CreateBasicClassInline(string typeName)
    {
        var typeNameRun = Run(typeName, CommonStyles.ClassMainBrush);
        return new SingleRunInline(typeNameRun);
    }
}

partial class BaseAnalysisNodeCreator
{
    /// <summary>
    /// An <see cref="AnalysisTreeListNode"/> creator, creating root nodes for the given
    /// values. The creator is provided access to the <see cref="BaseAnalysisNodeCreator"/>
    /// that encapsulates all other <see cref="RootViewNodeCreator{TValue, TCreator}"/>
    /// instances, allowing interaction between different types of nodes.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of the supported values represented as view nodes.
    /// </typeparam>
    /// <typeparam name="TCreator">
    /// The type of the <see cref="BaseAnalysisNodeCreator"/> instance that is
    /// accessed for further processing.
    /// </typeparam>
    /// <param name="creator"></param>
    public abstract class RootViewNodeCreator<TValue, TCreator>(TCreator creator)
        where TCreator : BaseAnalysisNodeCreator
    {
        public TCreator Creator { get; } = creator;

        public AnalysisNodeCreationOptions Options => Creator.Options;

        public virtual object? AssociatedSyntaxObject(TValue value) => value;

        public AnalysisTreeListNode CreateNode(
            TValue value, DisplayValueSource valueSource = default)
        {
            var rootLine = CreateNodeLine(value, valueSource);
            var children = GetChildRetriever(value);
            var syntaxObject = AssociatedSyntaxObject(value);
            return new AnalysisTreeListNode
            {
                NodeLine = rootLine,
                ChildRetriever = children,
                AssociatedSyntaxObjectContent = syntaxObject,
            };
        }

        public abstract AnalysisNodeChildRetriever? GetChildRetriever(TValue value);

        public abstract AnalysisTreeListNodeLine CreateNodeLine(
            TValue value, DisplayValueSource valueSource);
    }

    public abstract class GeneralValueRootViewNodeCreator<TValue>(BaseAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, BaseAnalysisNodeCreator>(creator)
    {
        public sealed override object? AssociatedSyntaxObject(TValue value)
        {
            return null;
        }
    }

    public sealed class GeneralRootViewNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object?>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object? value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var basicValueInline = BasicValueInline(value);
            inlines.Add(basicValueInline);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = CommonStyles.PropertyAccessValueDisplay,
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object? value)
        {
            if (value is null)
                return null;

            var type = value.GetType();
            if (IsSimpleType(type))
                return null;

            return () => GetChildren(value);
        }

        private SingleRunInline BasicValueInline(object? value)
        {
            if (value is null)
                return new(CreateNullValueRun());

            var type = value.GetType();
            bool isNullable = type.IsNullableValueType();
            if (isNullable)
            {
                object? innerValue = (value as dynamic).Value;
                return BasicValueInline(innerValue);
            }

            switch (type.GetTypeCode())
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                    break;

                default:
                    return Creator.RunForSimpleObjectValue(value);
            }

            var typeName = type.Name;
            return new(Run(typeName, CommonStyles.ClassMainBrush));
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(object value)
        {
            var type = value.GetType();

            if (IsSimpleType(type))
                return [];

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties
                .Where(FilterProperty)
                .Select(property => CreateFromProperty(property, value))
                .ToList()
                ;
        }

        private static bool FilterProperty(PropertyInfo property)
        {
            if (property.IsSpecialName)
                return false;

            var getter = property.GetGetMethod();
            if (getter is null)
                return false;

            int parameterCount = getter.GetParameters().Length;
            if (parameterCount is not 0)
                return false;

            return true;
        }

        private AnalysisTreeListNode CreateFromProperty(PropertyInfo property, object target)
        {
            var name = property.Name;
            var propertySource = Property(name);

            var value = property.GetValue(target);
            return Creator.CreateRootGeneral(value, propertySource);
        }
    }

    public sealed class BooleanRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<bool>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            bool value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var valueRun = SingleRunForBoolean(value);
            inlines.Add(valueRun);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = CommonStyles.PropertyAccessValueDisplay,
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(bool value)
        {
            return null;
        }

        private static SingleRunInline SingleRunForBoolean(bool value)
        {
            return new(RunForBoolean(value));
        }

        private static Run RunForBoolean(bool value)
        {
            return value switch
            {
                true => TrueRun(),
                false => FalseRun(),
            };
        }

        private static Run TrueRun()
        {
            return Run("true", CommonStyles.KeywordBrush);
        }

        private static Run FalseRun()
        {
            return Run("false", CommonStyles.KeywordBrush);
        }
    }

    public sealed class NullValueRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object?>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object? value, DisplayValueSource valueSource)
        {
            Debug.Assert(value is null);

            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var valueRun = CreateNullValueSingleRun();
            inlines.Add(valueRun);

            return new()
            {
                GroupedRunInlines = inlines,
                NodeTypeDisplay = CommonStyles.PropertyAccessValueDisplay,
            };
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object? value)
        {
            return null;
        }
    }
}

partial class BaseAnalysisNodeCreator
{
    public abstract class CommonTypes
    {
        public const string PropertyAccessValue = ".";
        public const string PropertyAnalysisValue = "";
    }

    public abstract class CommonStyles
    {
        public static readonly Color RawValueColor = Color.FromUInt32(0xFFE4E4E4);
        public static readonly SolidColorBrush RawValueBrush = new(RawValueColor);

        public static readonly Color KeywordColor = Color.FromUInt32(0xFF38A0FF);
        public static readonly SolidColorBrush KeywordBrush = new(KeywordColor);

        public static readonly Color NullValueColor = Color.FromUInt32(0xFF808080);
        public static readonly SolidColorBrush NullValueBrush = new(NullValueColor);

        public static readonly Color PropertyColor = Color.FromUInt32(0xFFE5986C);
        public static readonly SolidColorBrush PropertyBrush = new(PropertyColor);

        public static readonly Color MethodColor = Color.FromUInt32(0xFFFFF4B9);
        public static readonly SolidColorBrush MethodBrush = new(MethodColor);

        public static readonly Color SplitterColor = Color.FromUInt32(0xFFB4B4B4);
        public static readonly SolidColorBrush SplitterBrush = new(SplitterColor);

        public static readonly Color ClassMainColor = Color.FromUInt32(0xFF33E5A5);
        public static readonly SolidColorBrush ClassMainBrush = new(ClassMainColor);

        public static readonly Color ClassSecondaryColor = Color.FromUInt32(0xFF008052);
        public static readonly SolidColorBrush ClassSecondaryBrush = new(ClassSecondaryColor);

        public static readonly NodeTypeDisplay PropertyAnalysisValueDisplay
            = new(CommonTypes.PropertyAnalysisValue, RawValueColor);

        public static readonly NodeTypeDisplay PropertyAccessValueDisplay
            = new(CommonTypes.PropertyAccessValue, PropertyColor);
    }
}
