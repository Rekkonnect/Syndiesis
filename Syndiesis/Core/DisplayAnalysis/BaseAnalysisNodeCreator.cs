using Avalonia.Media;
using Garyon.Extensions;
using Garyon.Reflection;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using Syndiesis.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;
using GroupedRunInline = GroupedRunInline.IBuilder;
using KvpList = List<KeyValuePair<object, object?>>;
using Run = UIBuilder.Run;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using SingleRunInline = SingleRunInline.Builder;

public delegate IReadOnlyList<AnalysisTreeListNode> AnalysisNodeChildRetriever();

public abstract partial class BaseAnalysisNodeCreator
    : BaseInlineCreator
{
    // node creators
    private readonly GeneralLoadingRootViewNodeCreator _loadingCreator;
    private readonly GeneralRootViewNodeCreator _generalCreator;
    private readonly PrimitiveRootViewNodeCreator _primitiveCreator;
    private readonly NullValueRootAnalysisNodeCreator _nullValueCreator;
    private readonly BooleanRootAnalysisNodeCreator _booleanCreator;
    private readonly EnumRootAnalysisNodeCreator _enumCreator;
    private readonly EnumerableRootAnalysisNodeCreator _enumerableCreator;
    private readonly DictionaryRootAnalysisNodeCreator _dictionaryCreator;

    protected AnalysisNodeCreationOptions Options
        => AppSettings.Instance.NodeLineOptions;

    public readonly BaseAnalysisNodeCreatorContainer ParentContainer;

    public BaseAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
    {
        ParentContainer = parentContainer;

        _loadingCreator = new(this);
        _generalCreator = new(this);
        _primitiveCreator = new(this);
        _nullValueCreator = new(this);
        _booleanCreator = new(this);
        _enumCreator = new(this);
        _enumerableCreator = new(this);
        _dictionaryCreator = new(this);
    }

    public AnalysisTreeListNode? CreateRootViewNode(
        object? value, bool includeChildren)
    {
        return CreateRootViewNode<IDisplayValueSource>(value, null, includeChildren);
    }

    public AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource)
        where TDisplayValueSource : IDisplayValueSource
    {
        return CreateRootViewNode(value, valueSource, true);
    }

    public abstract AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren)
        where TDisplayValueSource : IDisplayValueSource
        ;

    public AnalysisTreeListNode CreateRootGeneral<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource = default, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return CreateRootViewNode(value, valueSource, includeChildren)
            ?? CreateRootBasic(value, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootBasic<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        if (value is null)
            return CreateRootNull(valueSource);

        switch (value)
        {
            case bool b:
                return _booleanCreator.CreateNode(b, valueSource, includeChildren);
        }

        var type = value.GetType();
        if (type.IsEnum)
        {
            return _enumCreator.CreateNode(value, valueSource, includeChildren);
        }

        if (type.IsNullableValueType())
        {
            return CreateRootNullableStruct(value as dynamic, valueSource, includeChildren);
        }

        if (IsPrimitiveType(type))
        {
            return _primitiveCreator.CreateNode(value, valueSource, includeChildren);
        }

        if (IsDictionaryType(type))
        {
            return _dictionaryCreator.CreateNode(value, valueSource, includeChildren);
        }

        if (SupportsEnumeration(type))
        {
            return _enumerableCreator.CreateNode(value, valueSource, includeChildren);
        }

        return _generalCreator.CreateNode(value, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootNull<TDisplayValueSource>(
        TDisplayValueSource? valueSource)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _nullValueCreator.CreateNode(null, valueSource);
    }

    private AnalysisTreeListNode CreateRootNullableStruct<T, TDisplayValueSource>(
        Nullable<T> value, TDisplayValueSource? valueSource, bool includeChildren = true)
        where T : struct
        where TDisplayValueSource : IDisplayValueSource
    {
        Debug.Assert(value is not null);

        var inner = value.Value;
        return CreateRootBasic(inner, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateLoadingNode<TDisplayValueSource>(
        Task<AnalysisTreeListNode?>? nodeTask,
        TDisplayValueSource? valueSource = default)
        where TDisplayValueSource : IDisplayValueSource
    {
        var node = _loadingCreator.CreateNode(valueSource);
        node.NodeLoader = nodeTask;
        return node;
    }

    private static bool IsDictionaryType(Type type)
    {
        var interfaces = type.GetInterfaces();
        return interfaces.Contains(typeof(IDictionary))
            || ContainsGenericVariant(interfaces, typeof(IDictionary<,>));
    }

    private static bool IsEnumerableType(Type type)
    {
        var interfaces = type.GetInterfaces();
        return interfaces.Contains(typeof(IEnumerable))
            || ContainsGenericVariant(interfaces, typeof(IEnumerable<>));
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

    protected static GroupedRunInline FullyQualifiedTypeDisplayGroupedRun(Type type)
    {
        var rightmost = NestedTypeDisplayGroupedRun(type);
        var fullNamespace = type.Namespace;

        if (string.IsNullOrEmpty(fullNamespace))
            return rightmost;

        var runList = new List<RunOrGrouped>();

        var namespaces = fullNamespace.Split('.');
        for (int i = 0; i < namespaces.Length; i++)
        {
            var inline = new SingleRunInline(Run(namespaces[i], CommonStyles.RawValueBrush));
            runList.Add(new(inline));
            runList.Add(CreateQualifierSeparatorRun());
        }

        runList.Add(new(rightmost));
        return new ComplexGroupedRunInline(runList);
    }

    protected static GroupedRunInline NestedTypeDisplayGroupedRun(Type type)
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

    protected static GroupedRunInline TypeDisplayGroupedRun(Type type)
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
                    innerRuns.Add(CreateArgumentSeparatorRun());
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

    private static SingleRunInline GetGeneralTypeDisplay(string typeName, LazilyUpdatedSolidBrush brush)
    {
        var typeNameRun = Run(typeName, brush);
        return new SingleRunInline(typeNameRun);
    }

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
    
    private static LazilyUpdatedSolidBrush GetBrushForTypeKind(Type type)
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

    protected static void AppendValueSource<TDisplayValueSource>(
        TDisplayValueSource? valueSource,
        GroupedRunInlineCollection inlines)
        where TDisplayValueSource : IDisplayValueSource
    {
        if (valueSource is null)
            return;

        switch (valueSource)
        {
            case DisplayValueSource displayValueSource:
                AppendValueSource(displayValueSource, inlines);
                break;

            case ComplexDisplayValueSource complexValueSource:
                AppendComplexValueSource(complexValueSource, inlines);
                break;
        }
    }

    protected static void AppendComplexValueSource(
        ComplexDisplayValueSource? valueSource,
        GroupedRunInlineCollection inlines)
    {
        var inline = NewComplexInlineGroup();
        AppendComplexValueSource(valueSource, inline);
        inlines.Add(inline);
    }

    protected static void AppendComplexValueSource(
        ComplexDisplayValueSource? valueSource,
        ComplexGroupedRunInline inlines)
    {
        if (valueSource is null)
            return;

        AppendValueSourceWithoutSplitter(valueSource, inlines);

        var colonRun = CreateValueSplitterRun();
        inlines.Children!.Add(colonRun);
    }

    private static ComplexGroupedRunInline NewComplexInlineGroup()
    {
        return new ComplexGroupedRunInline([]);
    }

    protected static void AppendValueSourceWithoutSplitter(
        ComplexDisplayValueSource? valueSource,
        ComplexGroupedRunInline inlines)
    {
        if (valueSource is null)
            return;

        var group = NewComplexInlineGroup();
        AppendValueSourceKindModifiers(valueSource.Modifiers, group);
        inlines.Children!.Add(new(group));

        bool hadPrevious = false;

        var currentSource = valueSource;
        while (currentSource is not null)
        {
            if (hadPrevious)
            {
                inlines.Children!.Add(CreateQualifierSeparatorRun());
            }

            AppendValueSourceWithoutSplitter(
                currentSource.Value,
                currentSource.Arguments,
                inlines);
            hadPrevious = true;
            currentSource = currentSource.Child;
        }
    }

    protected static void AppendValueSourceWithoutSplitter(
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

            case DisplayValueSource.SymbolKind.This:
                AppendThisDetail(inlines);
                break;

            default:
                return;
        }
    }

    protected static void AppendValueSourceWithoutSplitter(
        DisplayValueSource valueSource,
        ImmutableArray<ComplexDisplayValueSource> arguments,
        ComplexGroupedRunInline inlines)
    {
        if (valueSource.IsDefault)
            return;

        switch (valueSource.Kind)
        {
            case DisplayValueSource.SymbolKind.Property:
                AppendPropertyDetail(valueSource.Name!, inlines);
                break;

            case DisplayValueSource.SymbolKind.Method:
                AppendMethodDetail(valueSource, arguments, inlines);
                break;

            case DisplayValueSource.SymbolKind.This:
                AppendThisDetail(inlines);
                break;

            default:
                return;
        }
    }

    protected static void AppendValueSource(
        DisplayValueSource valueSource,
        GroupedRunInlineCollection inlines)
    {
        if (valueSource.IsDefault)
            return;

        AppendValueSourceWithoutSplitter(valueSource, inlines);

        var colonRun = CreateValueSplitterRun();
        inlines.Add(colonRun);
    }

    protected static void AppendMethodDetail(
        DisplayValueSource valueSource, GroupedRunInlineCollection inlines)
    {
        var frontGroup = CreateMethodDetailGroup(valueSource);
        inlines.Add(frontGroup);
    }

    protected static void AppendMethodDetail(
        DisplayValueSource valueSource, ComplexGroupedRunInline inline)
    {
        var frontGroup = CreateMethodDetailGroup(valueSource);
        inline.Children!.Add(new RunOrGrouped(frontGroup));
    }

    private static ComplexGroupedRunInline CreateMethodDetailGroup(DisplayValueSource valueSource)
    {
        var methodNameRun = Run(valueSource.Name!, CommonStyles.MethodBrush);
        var parenthesesRun = Run("()", CommonStyles.RawValueBrush);
        var frontGroup = new ComplexGroupedRunInline([
            methodNameRun,
            parenthesesRun,
        ]);

        AppendValueSourceKindModifiers(valueSource.Kind, frontGroup);
        return frontGroup;
    }

    protected static void AppendMethodDetail(
        DisplayValueSource valueSource,
        ImmutableArray<ComplexDisplayValueSource> arguments,
        ComplexGroupedRunInline inlines)
    {
        if (arguments.IsDefaultOrEmpty)
        {
            AppendMethodDetail(valueSource, inlines);
            return;
        }

        var methodNameRun = Run(valueSource.Name!, CommonStyles.MethodBrush);
        var openParenthesisRun = Run("(", CommonStyles.RawValueBrush);
        var closedParenthesisRun = Run(")", CommonStyles.RawValueBrush);
        var frontGroup = new ComplexGroupedRunInline([
            methodNameRun,
            openParenthesisRun,
        ]);

        AppendValueSourceKindModifiers(valueSource.Kind, frontGroup);

        var argumentGroup = NewComplexInlineGroup();
        for (int i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];
            if (i > 0)
            {
                var argumentSplitter = CreateArgumentSeparatorRun();
                frontGroup.Children!.Add(argumentSplitter);
            }
            AppendValueSourceWithoutSplitter(argument, argumentGroup);
        }

        frontGroup.Children!.Add(new RunOrGrouped(argumentGroup));
        frontGroup.Children!.Add(closedParenthesisRun);

        inlines.Children!.Add(new RunOrGrouped(frontGroup));
    }

    private static void AppendValueSourceKindModifiers(
        DisplayValueSource.SymbolKind kind, ComplexGroupedRunInline frontGroup)
    {
        if (kind.IsInternal())
        {
            var internalRun = CreateInternalRun();
            frontGroup.Children!.Insert(0, internalRun);
        }
        if (kind.IsAsync())
        {
            var awaitRun = CreateAwaitRun();
            frontGroup.Children!.Insert(0, awaitRun);
        }
    }

    protected static void AppendPropertyDetail(string propertyName, GroupedRunInlineCollection inlines)
    {
        var propertyNameRun = Run(propertyName, CommonStyles.PropertyBrush);
        inlines.AddSingle(propertyNameRun);
    }

    protected static void AppendThisDetail(GroupedRunInlineCollection inlines)
    {
        var thisRun = Run("this", CommonStyles.KeywordBrush);
        inlines.AddSingle(thisRun);
    }

    protected static void AppendPropertyDetail(string propertyName, ComplexGroupedRunInline inlines)
    {
        var propertyNameRun = Run(propertyName, CommonStyles.PropertyBrush);
        var single = new SingleRunInline(propertyNameRun);
        inlines.Children!.Add(new(single));
    }

    protected static void AppendThisDetail(ComplexGroupedRunInline inlines)
    {
        var thisRun = Run("this", CommonStyles.KeywordBrush);
        var single = new SingleRunInline(thisRun);
        inlines.Children!.Add(new(single));
    }

    protected static Run CreateValueSplitterRun()
    {
        return Run(":  ", CommonStyles.SplitterBrush);
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

    protected AnalysisTreeListNode CreateNodeForSimpleValue<TDisplayValueSource>(
        object? value,
        TDisplayValueSource? valueSource = default)
        where TDisplayValueSource : IDisplayValueSource
    {
        var line = LineForNodeValue(
            value,
            valueSource,
            CommonStyles.MemberAccessValueDisplay);
        return AnalysisTreeListNode(line, null, null);
    }

    protected AnalysisTreeListNodeLine LineForNodeValue<TDisplayValueSource>(
        object? value,
        TDisplayValueSource? valueSource,
        NodeTypeDisplay nodeTypeDisplay)
        where TDisplayValueSource : IDisplayValueSource
    {
        var inlines = new GroupedRunInlineCollection();

        AppendValueSource(valueSource, inlines);
        var valueRuns = RunForSimpleObjectValue(value);
        inlines.AddOneOrMany(valueRuns);

        return AnalysisTreeListNodeLine(
            inlines,
            CommonStyles.MemberAccessValueDisplay);
    }

    protected OneOrMany<RunOrGrouped> RunForSimpleObjectValue(object? value)
    {
        if (value is null)
            return new(CreateNullValueSingleRun());

        if (IsInteger(value))
        {
            return new(IntegerValueRuns(value));
        }

        var fullValue = value;

        if (value is string stringValue)
        {
            if (stringValue.Length is 0)
                return new(CreateEmptyValueSingleRun());

            value = SimplifyWhitespace(stringValue);
        }

        return new(RunForSimpleObjectValue(value, fullValue));
    }

    protected static SingleRunInline RunForSimpleObjectValue(object value, object fullValue)
    {
        var text = value.ToString()!;
        var fullText = fullValue!.ToString()!;
        var run = Run(text, CommonStyles.RawValueBrush);
        return new SingleRunInline(run, fullText);
    }

    private static bool IsInteger(object value)
    {
        switch (value.GetType().GetTypeCode())
        {
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:

            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
        }

        return false;
    }

    protected ImmutableArray<RunOrGrouped> IntegerValueRuns(object value)
    {
        var info = IntegerInfo.Create(value);
        return
        [
            SingleRun(value.ToString()!, ColorizationStyles.NumericLiteralBrush),
            CreateLargeSplitterRun(),
            CreateHexRunGroup(ref info),
            CreateLargeSplitterRun(),
            CreateBinaryRunGroup(ref info),
        ];
    }

    private static ComplexGroupedRunInline CreateHexRunGroup(
        ref readonly IntegerInfo info)
    {
        return CreateAlternativeNumericGroup("0x", HexIntegerWriter.Write(info, 4));
    }

    private static ComplexGroupedRunInline CreateBinaryRunGroup(
        ref readonly IntegerInfo info)
    {
        return CreateAlternativeNumericGroup("0b", BinaryIntegerWriter.Write(info, 4));
    }

    private static ComplexGroupedRunInline CreateAlternativeNumericGroup(
        string prefix, string representation)
    {
        var prefixRun = Run(prefix, ColorizationStyles.FadedNumericLiteralBrush);
        var display = SingleRun(representation, ColorizationStyles.NumericLiteralBrush);

        return new([prefixRun, display]);
    }

    protected static Run CreateAwaitRun()
    {
        return Run("await ", CommonStyles.KeywordBrush);
    }

    protected static Run CreateInternalRun()
    {
        return Run("internal ", CommonStyles.KeywordBrush);
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

    protected static GroupedRunInline? CountDisplayRunGroup(object value)
    {
        switch (value)
        {
            case Array array:
                return CountValueDisplay(array.Length, nameof(array.Length));

            case ICollection collection:
                return CountValueDisplay(collection.Count, nameof(collection.Count));
        }

        return CountDisplayRunGroup(value, nameof(ICollection.Count))
            ?? CountDisplayRunGroup(value, nameof(Array.Length));
    }

    protected static GroupedRunInline? CountDisplayRunGroup(object value, string propertyName)
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

    protected static GroupedRunInline CountValueDisplay(
        int count, string propertyName)
    {
        var propertyGroup = new SingleRunInline(Run(propertyName, CommonStyles.PropertyBrush));
        var separator = CreateValueSplitterRun();
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
        var splitter = CreateLargeSplitterRun();
        var display = CountValueDisplay(count, propertyName);
        inlines.AddRange([
            splitter,
            display,
        ]);
    }

    protected static SingleRunInline CreateEmptyValueSingleRun()
    {
        return new(CreateEmptyValueRun());
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

    protected static Run CreateLargeSplitterRun()
    {
        return Run("      ", CommonStyles.RawValueBrush);
    }

    protected static GroupedRunInline CreateBasicClassInline(string typeName)
    {
        var typeNameRun = Run(typeName, CommonStyles.ClassMainBrush);
        return new SingleRunInline(typeNameRun);
    }

    protected static GroupedRunInline TypeDisplayWithFadeSuffix(
        string typeName, string suffix, ILazilyUpdatedBrush main, ILazilyUpdatedBrush fade)
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

    protected static Run CreateThrowsRun()
    {
        return Run("throws ", CommonStyles.ThrowsBrush);
    }

    protected static GroupedRunInline ThrowsExceptionClause(
        string typeName)
    {
        var throws = CreateThrowsRun();
        var group = TypeDisplayWithFadeSuffix(
            typeName, "Exception",
            CommonStyles.ClassMainBrush, CommonStyles.ClassSecondaryBrush);

        return new ComplexGroupedRunInline([
            throws,
            new RunOrGrouped(group),
        ]);
    }

    protected AnalysisTreeListNodeLine CreateThrowsExceptionNodeLine(
        string typeName, DisplayValueSource valueSource)
    {
        var inlines = new GroupedRunInlineCollection();
        AppendValueSource(valueSource, inlines);
        var clause = ThrowsExceptionClause(typeName);
        inlines.Add(clause);
        return new(inlines, CommonStyles.ThrowsExceptionDisplay);
    }

    protected AnalysisTreeListNode CreateThrowsExceptionNode(
        string typeName, DisplayValueSource valueSource)
    {
        var line = CreateThrowsExceptionNodeLine(typeName, valueSource);
        return new(line, null, null);
    }

    protected AnalysisTreeListNode CreateGeneralOrThrowsExceptionNode<TException>(
        bool condition,
        Func<object?> getter,
        DisplayValueSource valueSource)
    {
        if (condition)
        {
            return CreateRootGeneral(getter(), valueSource);
        }
        else
        {
            var exceptionTypeName = typeof(TException).Name;
            return CreateThrowsExceptionNode(exceptionTypeName, valueSource);
        }
    }
}

partial class BaseAnalysisNodeCreator
{
    /// <summary>
    /// A <see cref="UIBuilder.AnalysisTreeListNode"/> creator, creating root nodes for the given
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
            TValue value, bool includeChildren)
        {
            return CreateNode<IDisplayValueSource>(value, null, includeChildren);
        }

        public AnalysisTreeListNode CreateNode<TDisplayValueSource>(
            TValue value, TDisplayValueSource? valueSource)
            where TDisplayValueSource : IDisplayValueSource
        {
            return CreateNode(value, valueSource, true);
        }

        public AnalysisTreeListNode CreateNode<TDisplayValueSource>(
            TValue value, TDisplayValueSource? valueSource, bool includeChildren)
            where TDisplayValueSource : IDisplayValueSource
        {
            var rootLine = CreateNodeLine(value, valueSource);
            var children = GetChildRetriever(value, includeChildren);
            var syntaxObject = AssociatedSyntaxObject(value);
            rootLine.AnalysisNodeKind = GetNodeKind(value);
            return AnalysisTreeListNode(
                rootLine,
                children,
                syntaxObject
            );
        }

        public AnalysisNodeChildRetriever? GetChildRetriever(TValue value, bool include)
        {
            if (!include)
                return null;

            return GetChildRetriever(value);
        }

        public abstract AnalysisNodeChildRetriever? GetChildRetriever(TValue value);

        public AnalysisTreeListNodeLine CreateNodeLine<TDisplayValueSource>(
            TValue value, TDisplayValueSource? valueSource)
            where TDisplayValueSource : IDisplayValueSource
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            return CreateNodeLine(value, inlines);
        }

        public abstract AnalysisTreeListNodeLine CreateNodeLine(
            TValue value, GroupedRunInlineCollection inlines);

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

    public sealed class GeneralLoadingRootViewNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object?>(creator)
    {
        public AnalysisTreeListNode CreateNode<TDisplayValueSource>(
            TDisplayValueSource? valueSource)
            where TDisplayValueSource : IDisplayValueSource
        {
            return CreateNode(null, valueSource);
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            object? value, GroupedRunInlineCollection inlines)
        {
            var loadingInline = NewLoadingInline();
            inlines.Add(loadingInline);

            var line = AnalysisTreeListNodeLine(
                inlines,
                default);

            line.IsLoading = true;

            return line;
        }

        private static SingleRunInline NewLoadingInline()
        {
            return new(new Run("Loading...", NodeCommonStyles.LoadingColorBrush));
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object? value)
        {
            return null;
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
            object value, GroupedRunInlineCollection inlines)
        {
            var basicValueInline = BasicValueInline(value);
            inlines.AddOneOrMany(basicValueInline);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
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

        private OneOrMany<RunOrGrouped> BasicValueInline(object? value)
        {
            if (value is null)
                return new(CreateNullValueSingleRun());

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

            return new(NestedTypeDisplayGroupedRun(type).AsRunOrGrouped);
        }

        private OneOrMany<RunOrGrouped> OptionalValueInline(object optional)
        {
            var type = optional.GetType();
            var hasValue = (bool)type.GetProperty(nameof(Optional<object>.HasValue))
                !.GetValue(optional)!;
            if (!hasValue)
            {
                var displayRun = CreateFadeNullLikeStateRun("[unspecified]");
                return new(new SingleRunInline(displayRun));
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
                .. BasicValueInline(key).Enumerable,
                CreateValueSplitterRun(),
                .. BasicValueInline(value).Enumerable,
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
            object? value, GroupedRunInlineCollection inlines)
        {
            var run = Creator.RunForSimpleObjectValue(value);
            inlines.AddOneOrMany(run);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
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
            bool value, GroupedRunInlineCollection inlines)
        {
            var valueRun = SingleRunForBoolean(value);
            inlines.Add(valueRun);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
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
            object value, GroupedRunInlineCollection inlines)
        {
            var typeRun = NestedTypeDisplayGroupedRun(value.GetType());
            inlines.Add(typeRun);
            inlines.Add(CreateQualifierSeparatorRun());
            var valueRun = EnumValueRun(value);
            inlines.AddSingle(valueRun);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(object value)
        {
            return null;
        }

        public static Run EnumValueRun<T>(T value)
            where T : notnull
        {
            return Run(value.ToString()!, CommonStyles.EnumFieldMainBrush);
        }
    }

    public sealed class NullValueRootAnalysisNodeCreator(BaseAnalysisNodeCreator creator)
        : GeneralValueRootViewNodeCreator<object?>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            object? value, GroupedRunInlineCollection inlines)
        {
            Debug.Assert(value is null);

            var valueRun = CreateNullValueSingleRun();
            inlines.Add(valueRun);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
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
                .Select(value => Creator.CreateRootGeneral(value, default(DisplayValueSource)))
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
                .Select(value => Creator.CreateRootGeneral(value, default(DisplayValueSource)))
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
            object value, GroupedRunInlineCollection inlines)
        {
            CreateNodeDisplayRuns(value, inlines);

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
        }

        protected void CreateNodeDisplayRuns(
            object value, GroupedRunInlineCollection inlines)
        {
            var typeRun = NestedTypeDisplayGroupedRun(value.GetType());
            inlines.Add(typeRun);

            var countDisplayRun = CountDisplayRunGroup(value);
            if (countDisplayRun is not null)
            {
                inlines.Add(CreateLargeSplitterRun());
                inlines.Add(countDisplayRun);
            }
        }
    }
}

partial class BaseAnalysisNodeCreator
{
    public abstract class CommonTypes
    {
        public const string MemberAccessValue = ".";
        public const string PropertyAnalysisValue = "";

        public const string ThrowsException = "X";
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
    [SolidColor("EnumMain", 0xFFB8D7A3)]
    [SolidColor("DelegateMain", 0xFF4BCBC8)]
    [SolidColor("ConstantMain", 0xFF7A68E5)]
    [SolidColor("EnumFieldMain", 0xFFE9A0FA)]
    [SolidColor("LocalMain", 0xFF88E9FF)]
    [SolidColor("IdentifierWildcard", 0xFF548C99)]
    [SolidColor("IdentifierWildcardFaded", 0xFF385E66)]
    [SolidColor("Throws", 0xFFB33E3E)]
    public sealed partial class NodeCommonStyles
    {
        public static readonly Color LoadingColor = Color.FromUInt32(0xFF007077);
        public static readonly LazilyUpdatedSolidBrush LoadingColorBrush = new(LoadingColor);

        public NodeTypeDisplay PropertyAnalysisValueDisplay
            => new(CommonTypes.PropertyAnalysisValue, RawValueColor);

        public NodeTypeDisplay MemberAccessValueDisplay
            => new(CommonTypes.MemberAccessValue, PropertyColor);

        public NodeTypeDisplay ThrowsExceptionDisplay
            => new(CommonTypes.ThrowsException, ThrowsColor);
    }
}
