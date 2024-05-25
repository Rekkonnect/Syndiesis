using Avalonia.Media;
using Avalonia.Threading;
using Garyon.Extensions;
using Garyon.Reflection;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

using KvpList = List<KeyValuePair<object, object?>>;
using static Syndiesis.Core.DisplayAnalysis.SyntaxAnalysisNodeCreator;

public delegate IReadOnlyList<AnalysisTreeListNode> AnalysisNodeChildRetriever();

public abstract partial class BaseAnalysisNodeCreator
{
    // node creators
    private readonly GeneralRootViewNodeCreator _generalCreator;
    private readonly PrimitiveRootViewNodeCreator _primitiveCreator;
    private readonly NullValueRootAnalysisNodeCreator _nullValueCreator;
    private readonly BooleanRootAnalysisNodeCreator _booleanCreator;
    private readonly EnumRootAnalysisNodeCreator _enumCreator;
    private readonly EnumerableRootAnalysisNodeCreator _enumerableCreator;
    private readonly DictionaryRootAnalysisNodeCreator _dictionaryCreator;

    protected readonly AnalysisNodeCreationOptions Options;

    public readonly AnalysisNodeCreatorContainer ParentContainer;

    public BaseAnalysisNodeCreator(
        AnalysisNodeCreationOptions options,
        AnalysisNodeCreatorContainer parentContainer)
    {
        Options = options;
        ParentContainer = parentContainer;

        _generalCreator = new(this);
        _primitiveCreator = new(this);
        _nullValueCreator = new(this);
        _booleanCreator = new(this);
        _enumCreator = new(this);
        _enumerableCreator = new(this);
        _dictionaryCreator = new(this);
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

        switch (value)
        {
            case bool b:
                return _booleanCreator.CreateNode(b, valueSource);
        }

        var type = value.GetType();
        if (type.IsEnum)
        {
            return _enumCreator.CreateNode(value, valueSource);
        }

        if (type.IsNullableValueType())
        {
            return CreateRootNullableStruct(value as dynamic, valueSource);
        }

        if (IsPrimitiveType(type))
        {
            return _primitiveCreator.CreateNode(value, valueSource);
        }

        if (IsDictionaryType(type))
        {
            return _dictionaryCreator.CreateNode(value, valueSource);
        }

        if (SupportsEnumeration(type))
        {
            return _enumerableCreator.CreateNode(value, valueSource);
        }

        return _generalCreator.CreateNode(value, valueSource);
    }

    private static bool IsDictionaryType(Type type)
    {
        if (type is IDictionary)
            return true;

        return ContainsGenericVariant(type.GetInterfaces(), typeof(IDictionary<,>));
    }

    private static bool IsEnumerableType(Type type)
    {
        return type is IEnumerable;
    }

    private static bool SupportsEnumeration(Type type)
    {
        if (IsEnumerableType(type))
            return true;

        if (type.GetMethod(nameof(IEnumerable.GetEnumerator)) is not null)
            return true;

        return false;
    }

    private static bool ContainsGenericVariant(IReadOnlyList<Type> types, Type targetGeneric)
    {
        foreach (var type in types)
        {
            if (type.IsGenericVariantOf(targetGeneric))
                return true;
        }
        return false;
    }

    protected GroupedRunInline NestedTypeDisplayGroupedRun(Type type)
    {
        var rightmost = TypeDisplayGroupedRun(type);
        var runList = new List<RunOrGrouped> { new(rightmost) };

        var outer = type.DeclaringType;
        while (outer is not null)
        {
            runList.Add(CreateQualifierSeparatorRun());
            runList.Add(new(TypeDisplayGroupedRun(outer)));
            outer = outer.DeclaringType;
        }

        if (runList.Count is 1)
            return rightmost;

        runList.Reverse();
        return new ComplexGroupedRunInline(runList);
    }

    protected GroupedRunInline TypeDisplayGroupedRun(Type type)
    {
        var brush = GetBrushForTypeKind(type);
        if (type.IsGenericType)
        {
            var originalDefinition = type.GetGenericTypeDefinition();
            originalDefinition.Name.AsSpan().SplitOnce('`', out var name, out var aritySuffix);
            var outerRun = Run($"{name}<", brush);
            var closingTag = Run(">", brush);
            var innerRuns = new List<RunOrGrouped>();
            var arguments = type.GenericTypeArguments;
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var inner = TypeDisplayGroupedRun(argument);
                innerRuns.Add(new(inner));

                if (i < arguments.Length - 1)
                {
                    innerRuns.Add(CreateGenericArgumentSeparatorRun());
                }
            }

            return new ComplexGroupedRunInline([
                outerRun,
                .. innerRuns,
                closingTag,
            ]);
        }

        return GetPrimitiveTypeAliasDisplay(type)
            ?? GetGeneralTypeDisplay(type.Name, brush);
    }

    private static SingleRunInline? GetPrimitiveTypeAliasDisplay(Type type)
    {
        var brush = CommonStyles.KeywordBrush;
        var alias = GetTypeAlias(type);
        if (alias is null)
            return null;
        return new(Run(alias, brush));
    }

    private static SingleRunInline GetGeneralTypeDisplay(string typeName, SolidColorBrush brush)
    {
        var typeNameRun = Run(typeName, brush);
        return new SingleRunInline(typeNameRun);
    }

#if false // generation of the code below
using System;

var types = """
    byte
    short
    int
    long
    sbyte
    ushort
    uint
    ulong
    float
    double
    decimal
    string
    char
    bool
    object
    void
    """;

var lines = types.AsSpan().EnumerateLines();
foreach (var line in lines)
{
    Console.WriteLine(Code(line.ToString()));
}

static string Code(string type)
{
    return $$"""
        if (type == typeof({{type}}))
            return "{{type}}";

        """;
}
#endif

    private static bool IsPrimitiveType(Type type)
    {
        switch (type.GetTypeCode())
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.String:
            case TypeCode.Char:
            case TypeCode.Boolean:
                return true;
        }

        return type == typeof(object)
            || type == typeof(void)
            ;
    }

    private static string? GetTypeAlias(Type type)
    {
        if (type == typeof(byte))
            return "byte";

        if (type == typeof(short))
            return "short";

        if (type == typeof(int))
            return "int";

        if (type == typeof(long))
            return "long";

        if (type == typeof(sbyte))
            return "sbyte";

        if (type == typeof(ushort))
            return "ushort";

        if (type == typeof(uint))
            return "uint";

        if (type == typeof(ulong))
            return "ulong";

        if (type == typeof(float))
            return "float";

        if (type == typeof(double))
            return "double";

        if (type == typeof(decimal))
            return "decimal";

        if (type == typeof(string))
            return "string";

        if (type == typeof(char))
            return "char";

        if (type == typeof(bool))
            return "bool";

        if (type == typeof(object))
            return "object";

        if (type == typeof(void))
            return "void";

        return null;
    }

    private static SolidColorBrush GetBrushForTypeKind(Type type)
    {
        if (type.IsEnum)
            return CommonStyles.EnumMainBrush;

        if (type.IsInterface)
            return CommonStyles.InterfaceMainBrush;

        if (type.IsDelegate())
            return CommonStyles.DelegateMainBrush;

        if (type.IsClass)
            return CommonStyles.ClassMainBrush;

        if (type.IsValueType)
            return CommonStyles.StructMainBrush;

        if (type.IsVoid())
            return CommonStyles.KeywordBrush;

        throw new UnreachableException("Type kinds should have all been evaluated here");
    }

    private static Run CreateGenericArgumentSeparatorRun()
    {
        return Run(", ", CommonStyles.RawValueBrush);
    }

    private static Run CreateQualifierSeparatorRun()
    {
        return Run(".", CommonStyles.RawValueBrush);
    }

    // known unspeakable characters on compiler-generated names
    private static readonly SearchValues<char> _unspeakableChars = SearchValues.Create("<>$#");

    private static bool IsUnspeakableName(string name)
    {
        return name.AsSpan().ContainsAny(_unspeakableChars);
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
            frontGroup.Children!.Insert(0, awaitRun);
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
        var line = LineForNodeValue(
            value,
            valueSource,
            CommonStyles.PropertyAccessValueDisplay);
        return AnalysisTreeListNode(line, null, null);
    }

    protected AnalysisTreeListNodeLine LineForNodeValue(
        object? value,
        DisplayValueSource valueSource,
        NodeTypeDisplay nodeTypeDisplay)
    {
        var inlines = new GroupedRunInlineCollection();

        AppendValueSource(valueSource, inlines);
        var valueRun = RunForSimpleObjectValue(value);
        inlines.Add(valueRun);

        return AnalysisTreeListNodeLine(
            inlines,
            CommonStyles.PropertyAccessValueDisplay);
    }

    protected SingleRunInline RunForSimpleObjectValue(object? value)
    {
        if (value is null)
            return new SingleRunInline(CreateNullValueRun());

        var fullValue = value;

        if (value is string stringValue)
        {
            if (stringValue.Length is 0)
                return new SingleRunInline(CreateEmptyValueRun());

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
        return new(text, brush);
    }

    protected static Run Run(string text, IBrush brush, FontStyle fontStyle)
    {
        return new(
            text,
            brush,
            fontStyle);
    }

    protected static AnalysisTreeListNodeLine AnalysisTreeListNodeLine(
        GroupedRunInlineCollection inlines,
        NodeTypeDisplay nodeTypeDisplay)
    {
        return new(
            inlines,
            nodeTypeDisplay);
    }

    protected static AnalysisTreeListNode AnalysisTreeListNode(
        AnalysisTreeListNodeLine nodeLine,
        AnalysisNodeChildRetriever? childRetriever,
        object? associatedSyntaxObjectContent)
    {
        return new(
            nodeLine,
            childRetriever,
            associatedSyntaxObjectContent);
    }

    protected static DisplayValueSource Property(string name)
    {
        return new(DisplayValueSource.SymbolKind.Property, name);
    }

    protected static DisplayValueSource MethodSource(string name)
    {
        return new(DisplayValueSource.SymbolKind.Method, name);
    }

    protected static GroupedRunInline CountValueDisplay(
        int count, string propertyName)
    {
        var propertyGroup = new SingleRunInline(Run(propertyName, CommonStyles.PropertyBrush));
        var separator = Run(":  ", CommonStyles.SplitterBrush);
        var countRun = Run(count.ToString(), CommonStyles.RawValueBrush);
        return new ComplexGroupedRunInline([
            new(propertyGroup),
                separator,
                countRun,
            ]);
    }

    protected static void AppendCountValueDisplay(
        GroupedRunInlineCollection inlines,
        int count,
        string propertyName)
    {
        var splitter = NewValueKindSplitterRun();
        var display = CountValueDisplay(count, propertyName);
        inlines.AddRange([
            splitter,
            display,
        ]);
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
        return CreateFadeNullLikeStateRun(nullDisplay);
    }

    protected static Run CreateFadeNullLikeStateRun(string displayText)
    {
        return Run(
            displayText,
            CommonStyles.NullValueBrush,
            FontStyle.Italic);
    }

    protected static Run CreateNullKeywordRun()
    {
        const string nullDisplay = "null";
        return Run(
            nullDisplay,
            CommonStyles.KeywordBrush);
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

    protected static GroupedRunInline TypeDisplayWithFadeSuffix(
        string typeName, string suffix, IBrush main, IBrush fade)
    {
        if (typeName.EndsWith(suffix))
        {
            var primaryName = Run(typeName[..^suffix.Length], main);
            var suffixNameRun = Run(suffix, fade);

            return new SimpleGroupedRunInline([
                primaryName,
                suffixNameRun,
            ]);
        }

        return new SingleRunInline(Run(typeName, main));
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
    public abstract class RootViewNodeCreator<TValue, TCreator>(TCreator creator)
        where TCreator : BaseAnalysisNodeCreator
    {
        public TCreator Creator { get; } = creator;

        public AnalysisNodeCreationOptions Options => Creator.Options;

        public virtual object? AssociatedSyntaxObject(TValue value) => value;

        public virtual AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.None;
        }

        public AnalysisTreeListNode CreateNode(
            TValue value, DisplayValueSource valueSource = default)
        {
            var rootLine = CreateNodeLine(value, valueSource);
            var children = GetChildRetriever(value);
            var syntaxObject = AssociatedSyntaxObject(value);
            rootLine.AnalysisNodeKind = GetNodeKind(value);
            return AnalysisTreeListNode(
                rootLine,
                children,
                syntaxObject
            );
        }

        public abstract AnalysisNodeChildRetriever? GetChildRetriever(TValue value);

        public abstract AnalysisTreeListNodeLine CreateNodeLine(
            TValue value, DisplayValueSource valueSource);

        protected AnalysisTreeListNode CreateFromProperty(PropertyInfo property, object target)
        {
            ExtractPropertyDisplayValues(
                property, target, out var value, out var propertySource);
            return Creator.CreateRootGeneral(value, propertySource);
        }

        protected AnalysisTreeListNode CreateFromPropertyWithSyntaxObject(
            PropertyInfo property, object target)
        {
            ExtractPropertyDisplayValues(
                property, target, out var value, out var propertySource);
            var node = CreateFromProperty(property, target);
            if (node.AssociatedSyntaxObject is not null)
                return node;

            return node.WithAssociatedSyntaxObjectContent(value);
        }

        private static void ExtractPropertyDisplayValues(
            PropertyInfo property,
            object target,
            out object? value,
            out DisplayValueSource propertySource)
        {
            var name = property.Name;
            propertySource = Property(name);
            value = property.GetValue(target);
        }
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
        : GeneralValueRootViewNodeCreator<object>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var basicValueInline = BasicValueInline(value);
            inlines.Add(basicValueInline);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object value)
        {
            if (value is null)
                return null;

            var type = value.GetType();
            if (IsSimpleType(type))
                return null;

            return () => GetChildren(value);
        }

        private GroupedRunInline BasicValueInline(object? value)
        {
            if (value is null)
                return new SingleRunInline(CreateNullValueRun());

            var type = value.GetType();
            var genericDeclaration = type.GetGenericTypeDefinitionOrSame();
            bool isNullable = genericDeclaration == typeof(Nullable<>);
            if (isNullable)
            {
                object? innerValue = (value as dynamic).Value;
                return BasicValueInline(innerValue);
            }

            var isKvp = genericDeclaration == typeof(KeyValuePair<,>);
            if (isKvp)
            {
                return KvpValueInline(value as dynamic);
            }

            var isOptional = genericDeclaration == typeof(Optional<>);
            if (isOptional)
            {
                // using dynamic to bind to a generic function automatically
                // converts the value into an Optional<object> with a [null]
                // value instead of an [unspecified] one
                return OptionalValueInline(value);
            }

            switch (type.GetTypeCode())
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                    break;

                default:
                    return Creator.RunForSimpleObjectValue(value);
            }

            return Creator.NestedTypeDisplayGroupedRun(type);
        }

        private GroupedRunInline OptionalValueInline(object optional)
        {
            var type = optional.GetType();
            var hasValue = (bool)type.GetProperty(nameof(Optional<object>.HasValue))
                !.GetValue(optional)!;
            if (!hasValue)
            {
                var displayRun = CreateFadeNullLikeStateRun("[unspecified]");
                return new SingleRunInline(displayRun);
            }

            var value = type.GetProperty(nameof(Optional<object>.Value))
                !.GetValue(optional);
            return BasicValueInline(value);
        }

        private GroupedRunInline KvpValueInline<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            return new ComplexGroupedRunInline([
                new(BasicValueInline(key)),
                Run(":  ", CommonStyles.SplitterBrush),
                new(BasicValueInline(value)),
            ]);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(object value)
        {
            var type = value.GetType();

            if (IsSimpleType(type))
                return [];

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties
                .Where(FilterProperty)
                .OrderBy(s => s.Name)
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
    }

    public sealed class PrimitiveRootViewNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object?>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object? value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var run = Creator.RunForSimpleObjectValue(value);
            inlines.Add(run);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object? value)
        {
            return null;
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

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
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

    public sealed class EnumRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var typeRun = Creator.NestedTypeDisplayGroupedRun(value.GetType());
            inlines.Add(typeRun);
            inlines.Add(CreateQualifierSeparatorRun());
            var valueRun = EnumValueRun(value);
            inlines.AddSingle(valueRun);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object value)
        {
            return null;
        }

        private static Run EnumValueRun(object value)
        {
            return Run(value.ToString()!, CommonStyles.ConstantMainBrush);
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

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object? value)
        {
            return null;
        }
    }

    public sealed class EnumerableRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : BaseEnumerableRootAnalysisNodeCreator(creator)
    {
        public override AnalysisNodeChildRetriever? GetChildRetriever(object value)
        {
            var enumerable = Flatten(value);
            if (enumerable.Count is 0)
                return null;

            return () => GetChildNodes(enumerable);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildNodes(IReadOnlyList<object> values)
        {
            return values
                .Select(value => Creator.CreateRootGeneral(value))
                .ToList()
                ;
        }

        private static List<object> Flatten(object source)
        {
            if (source is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().ToListOrExisting();
            }

            return EnumerateDynamic(source);
        }

        private static List<object> EnumerateDynamic(dynamic enumerable)
        {
            var result = new List<object>();
            foreach (var value in enumerable)
            {
                result.Add(value);
            }
            return result;
        }
    }

    public sealed class DictionaryRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : BaseEnumerableRootAnalysisNodeCreator(creator)
    {
        public override AnalysisNodeChildRetriever? GetChildRetriever(object value)
        {
            var enumerable = Flatten(value);
            if (enumerable.Count is 0)
                return null;

            return () => GetChildNodes(enumerable);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildNodes(KvpList values)
        {
            return values
                .Select(value => Creator.CreateRootGeneral(value))
                .ToList()
                ;
        }

        private static KvpList Flatten(object source)
        {
            if (source is IDictionary dictionary)
            {
                var result = new KvpList();
                foreach (var key in dictionary.Keys)
                {
                    result.Add(new(key, dictionary[key]));
                }
                return result;
            }

            return FlattenGeneric(source as dynamic);
        }

        private static KvpList FlattenGeneric<TKey, TValue>(
            IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
        {
            var list = new KvpList();

            foreach (var kvp in dictionary)
            {
                list.Add(new(kvp.Key!, kvp.Value));
            }

            return list;
        }

        private static List<object> EnumerateDynamic(dynamic enumerable)
        {
            var result = new List<object>();
            foreach (var value in enumerable)
            {
                result.Add(value);
            }
            return result;
        }
    }

    public abstract class BaseEnumerableRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object value, DisplayValueSource valueSource)
        {
            var inlines = CreateNodeDisplayRuns(value, valueSource);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.PropertyAccessValueDisplay);
        }

        protected GroupedRunInlineCollection CreateNodeDisplayRuns(
            object value, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();

            Creator.AppendValueSource(valueSource, inlines);
            var typeRun = Creator.NestedTypeDisplayGroupedRun(value.GetType());
            inlines.Add(typeRun);

            var countDisplayRun = CountDisplayRunGroup(value);
            if (countDisplayRun is not null)
            {
                inlines.Add(NewValueKindSplitterRun());
                inlines.Add(countDisplayRun);
            }

            return inlines;
        }

        protected static GroupedRunInline? CountDisplayRunGroup(object value)
        {
            return CountDisplayRunGroup(value, nameof(ICollection.Count))
                ?? CountDisplayRunGroup(value, nameof(Array.Length));
        }

        private static GroupedRunInline? CountDisplayRunGroup(object value, string propertyName)
        {
            var type = value.GetType();
            var property = type.GetProperty(propertyName);
            if (property is null)
                return null;

            if (property.PropertyType != typeof(int))
                return null;

            int count = (int)property.GetValue(value)!;
            return CountValueDisplay(count, propertyName);
        }
    }
}

partial class BaseAnalysisNodeCreator
{
    public static NodeCommonStyles CommonStyles { get; }
        = Dispatcher.UIThread.ExecuteOrDispatch<NodeCommonStyles>(() => new());

    public abstract class CommonTypes
    {
        public const string PropertyAccessValue = ".";
        public const string PropertyAnalysisValue = "";
    }

    [SolidColor("RawValue", 0xFFE4E4E4)]
    [SolidColor("Keyword", 0xFF38A0FF)]
    [SolidColor("NullValue", 0xFF808080)]
    [SolidColor("Property", 0xFFE5986C)]
    [SolidColor("Method", 0xFFFFF4B9)]
    [SolidColor("Splitter", 0xFFB4B4B4)]
    [SolidColor("ClassMain", 0xFF33E5A5)]
    [SolidColor("ClassSecondary", 0xFF008052)]
    [SolidColor("StructMain", 0xFF4DCA85)]
    [SolidColor("InterfaceMain", 0xFFA2D080)]
    [SolidColor("InterfaceSecondary", 0xFF6D8C57)]
    [SolidColor("EnumMain", 0XFFB8D7A3)]
    [SolidColor("DelegateMain", 0xFF4BCBC8)]
    [SolidColor("ConstantMain", 0xFF7A68E5)]
    [SolidColor("LocalMain", 0xFF88E9FF)]
    [SolidColor("IdentifierWildcard", 0xFF548C99)]
    [SolidColor("IdentifierWildcardFaded", 0xFF385E66)]
    public sealed partial class NodeCommonStyles
    {
        public NodeTypeDisplay PropertyAnalysisValueDisplay
            => new(CommonTypes.PropertyAnalysisValue, RawValueColor);

        public NodeTypeDisplay PropertyAccessValueDisplay
            => new(CommonTypes.PropertyAccessValue, PropertyColor);
    }
}
