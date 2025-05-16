using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;
using SingleRunInline = SingleRunInline.Builder;

public sealed partial class SemanticModelAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    private static readonly InterestingPropertyFilterCache _propertyCache
        = new(SemanticModelPropertyFilter.Instance);

    // node creators
    private readonly SemanticModelRootViewNodeCreator _semanticModelCreator;
    private readonly TypeInfoRootViewNodeCreator _typeInfoCreator;
    private readonly SymbolInfoRootViewNodeCreator _symbolInfoCreator;
    private readonly NullabilityInfoRootViewNodeCreator _nullabilityInfoCreator;
    private readonly PreprocessingSymbolInfoRootViewNodeCreator _preprocessingSymbolInfoCreator;
    private readonly CSharpConversionRootViewNodeCreator _cSharpConversionCreator;
    private readonly VisualBasicConversionRootViewNodeCreator _visualBasicConversionCreator;

    public SemanticModelAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _semanticModelCreator = new(this);
        _typeInfoCreator = new(this);
        _symbolInfoCreator = new(this);
        _nullabilityInfoCreator = new(this);
        _preprocessingSymbolInfoCreator = new(this);
        _cSharpConversionCreator = new(this);
        _visualBasicConversionCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case SemanticModel semanticModel:
                return CreateRootSemanticModel(semanticModel, valueSource, includeChildren);

            case TypeInfo typeInfo:
                return CreateRootTypeInfo(typeInfo, valueSource, includeChildren);

            case SymbolInfo symbolInfo:
                return CreateRootSymbolInfo(symbolInfo, valueSource, includeChildren);

            case NullabilityInfo nullabilityInfo:
                return CreateRootNullabilityInfo(nullabilityInfo, valueSource, includeChildren);

            case PreprocessingSymbolInfo preprocessingSymbolInfo:
                return CreateRootPreprocessingSymbolInfo(
                    preprocessingSymbolInfo,
                    valueSource,
                    includeChildren);

            case CSharpConversion conversion:
                return CreateRootConversion(conversion, valueSource, includeChildren);

            case VisualBasicConversion conversion:
                return CreateRootConversion(conversion, valueSource, includeChildren);

            // internal API to wrap around the lack of abstraction for conversions
            case ConversionUnion conversion:
                return CreateRootConversion(conversion, valueSource, includeChildren);

            default:
                break;
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource);
    }

    public AnalysisTreeListNode CreateRootSemanticModel<TDisplayValueSource>(
        SemanticModel semanticModel,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _semanticModelCreator.CreateNode(semanticModel, valueSource);
    }

    public AnalysisTreeListNode CreateRootTypeInfo<TDisplayValueSource>(
        TypeInfo typeInfo,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _typeInfoCreator.CreateNode(typeInfo, valueSource);
    }

    public AnalysisTreeListNode CreateRootSymbolInfo<TDisplayValueSource>(
        SymbolInfo symbolInfo,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _symbolInfoCreator.CreateNode(symbolInfo, valueSource);
    }

    public AnalysisTreeListNode CreateRootNullabilityInfo<TDisplayValueSource>(
        NullabilityInfo nullabilityInfo,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _nullabilityInfoCreator.CreateNode(nullabilityInfo, valueSource);
    }

    public AnalysisTreeListNode CreateRootPreprocessingSymbolInfo<TDisplayValueSource>(
        PreprocessingSymbolInfo preprocessingSymbolInfo,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _preprocessingSymbolInfoCreator.CreateNode(preprocessingSymbolInfo, valueSource);
    }

    public AnalysisTreeListNode CreateRootConversion<TDisplayValueSource>(
        CSharpConversion conversion,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _cSharpConversionCreator.CreateNode(conversion, valueSource);
    }

    public AnalysisTreeListNode CreateRootConversion<TDisplayValueSource>(
        VisualBasicConversion conversion,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _visualBasicConversionCreator.CreateNode(conversion, valueSource);
    }

    public AnalysisTreeListNode CreateRootConversion<TDisplayValueSource>(
        ConversionUnion conversionUnion,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return CreateRootGeneral(conversionUnion.AppliedConversion, valueSource);
    }
}

partial class SemanticModelAnalysisNodeCreator
{
    public abstract class SemanticModelRootViewNodeCreator<TValue>(SemanticModelAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, SemanticModelAnalysisNodeCreator>(creator)
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.None;
        }

        protected void AddPropertyNodeIfTrue(
            List<AnalysisTreeListNode> list, bool value, string name)
        {
            if (!value)
                return;

            var node = Creator.CreateRootBasic(value, Property(name));
            list.Add(node);
        }
    }

    public sealed class SemanticModelRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<SemanticModel>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SemanticModel model, GroupedRunInlineCollection inlines)
        {
            var type = model.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SemanticModelDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SemanticModel model)
        {
            return () => GetChildren(model);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SemanticModel model)
        {
            var type = _propertyCache.FilterForType(model.GetType());
            var properties = type.Properties;
            var preferredType = type.PreferredType;

            return properties
                .OrderBy(s => s.Name)
                .Select(property => CreateFromProperty(property, model))
                .ToList()
                ;
        }
    }

    public sealed class TypeInfoRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<TypeInfo>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TypeInfo type, GroupedRunInlineCollection inlines)
        {
            var inline = NestedTypeDisplayGroupedRun(typeof(TypeInfo));
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TypeInfoDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(TypeInfo type)
        {
            return () => GetChildren(type);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(TypeInfo type)
        {
            return
            [
                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    type.Type,
                    Property(nameof(TypeInfo.Type))),

                Creator.CreateRootGeneral(
                    type.Nullability,
                    Property(nameof(TypeInfo.Nullability))),

                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    type.ConvertedType,
                    Property(nameof(TypeInfo.ConvertedType))),

                Creator.CreateRootGeneral(
                    type.ConvertedNullability,
                    Property(nameof(TypeInfo.ConvertedNullability))),
            ];
        }
    }

    public sealed class SymbolInfoRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<SymbolInfo>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SymbolInfo symbol, GroupedRunInlineCollection inlines)
        {
            AddQuickDisplayInlines(symbol, inlines);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SymbolInfoDisplay);
        }

        private void AddQuickDisplayInlines(SymbolInfo info, GroupedRunInlineCollection inlines)
        {
            if (info.Symbol is not null)
            {
                AddSymbolInlines(info.Symbol, inlines);
            }
            else
            {
                AddCandidateSymbolInlines(info, inlines);
            }
        }

        private void AddSymbolInlines(ISymbol symbol, GroupedRunInlineCollection inlines)
        {
            var creator = Creator.ParentContainer.SymbolCreator.CreatorForSymbol(symbol);
            creator?.AddSummaryInlines(symbol, inlines);
        }

        private void AddCandidateSymbolInlines(SymbolInfo symbol, GroupedRunInlineCollection inlines)
        {
            var rawTextBrush = CommonStyles.RawValueBrush;

            var candidates = symbol.CandidateSymbols;
            if (candidates.IsDefaultOrEmpty)
            {
                var noCandidatesRun = Run("0 candidates", rawTextBrush);
                inlines.Add(noCandidatesRun);
                return;
            }

            int count = candidates.Length;
            var run = Run(CandidatesString(count), rawTextBrush);
            inlines.Add(run);

            if (symbol.CandidateReason is not CandidateReason.None)
            {
                var viaRun = Run(" due to ", rawTextBrush);
                inlines.Add(viaRun);

                var reasonRun = Run(
                    symbol.CandidateReason.ToString(),
                    CommonStyles.EnumFieldMainBrush);
                var reasonInline = new SingleRunInline(reasonRun);
                inlines.Add(reasonInline);
            }
        }

        private static string CandidatesString(int count)
        {
            return $"{count} {CandidatesFormString(count)}";
        }

        private static string CandidatesFormString(int count)
        {
            if (count is 1)
                return "candidate";

            return "candidates";
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SymbolInfo symbol)
        {
            return () => GetChildren(symbol);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SymbolInfo symbol)
        {
            return
            [
                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    symbol.Symbol,
                    Property(nameof(SymbolInfo.Symbol))),

                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    symbol.CandidateSymbols,
                    Property(nameof(SymbolInfo.CandidateSymbols))),

                Creator.CreateRootBasic(
                    symbol.CandidateReason,
                    Property(nameof(SymbolInfo.CandidateReason))),
            ];
        }
    }

    public sealed class NullabilityInfoRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<NullabilityInfo>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            NullabilityInfo nullability, GroupedRunInlineCollection inlines)
        {
            var inline = NestedTypeDisplayGroupedRun(typeof(NullabilityInfo));
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.NullabilityInfoDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(NullabilityInfo nullability)
        {
            return () => GetChildren(nullability);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(NullabilityInfo nullability)
        {
            return
            [
                Creator.CreateRootBasic(
                    nullability.Annotation,
                    Property(nameof(NullabilityInfo.Annotation))),

                Creator.CreateRootBasic(
                    nullability.FlowState,
                    Property(nameof(NullabilityInfo.FlowState))),
            ];
        }
    }

    public sealed class PreprocessingSymbolInfoRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<PreprocessingSymbolInfo>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            PreprocessingSymbolInfo preprocessing, GroupedRunInlineCollection inlines)
        {
            AddQuickDisplayInlines(preprocessing, inlines);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.PreprocessingSymbolInfoDisplay);
        }

        private void AddQuickDisplayInlines(
            PreprocessingSymbolInfo info, GroupedRunInlineCollection inlines)
        {
            if (info.Symbol is null)
            {
                AddNoSymbolInlines(inlines);
            }
            else
            {
                AddPreprocessingSymbolInlines(info, inlines);
            }
        }

        private void AddNoSymbolInlines(GroupedRunInlineCollection inlines)
        {
            var noSymbolsRun = Run("[none]", TriviaStyles.WhitespaceTriviaBrush);
            var noSymbolsInline = new SingleRunInline(noSymbolsRun);
            inlines.Add(noSymbolsInline);
        }

        private void AddPreprocessingSymbolInlines(
            PreprocessingSymbolInfo symbol, GroupedRunInlineCollection inlines)
        {
            var name = symbol.Symbol!.Name;

            var preprocessingSymbolRun = Run(
                name,
                AppSettings.Instance.ColorizationPreferences.ColorizationStyles!.PreprocessingBrush);
            var preprocessingSymbolInline = new SingleRunInline(preprocessingSymbolRun);
            inlines.Add(preprocessingSymbolInline);

            var splitter = CreateLargeSplitterRun();
            inlines.Add(splitter);

            var definedRun = NewDefinedRun(symbol.IsDefined);
            inlines.Add(definedRun);
        }

        private static BaseSyntaxAnalysisNodeCreator.SyntaxStyles TriviaStyles
            => AppSettings.Instance.NodeColorPreferences.SyntaxStyles!;

        private static SingleRunInline NewDefinedRun(bool defined)
        {
            return defined
                ? NewDefinedRun()
                : NewUndefinedRun()
                ;
        }

        private static SingleRunInline NewUndefinedRun()
        {
            var run = Run("[undefined]", TriviaStyles.BasicTriviaNodeTypeBrush);
            return new SingleRunInline(run);
        }

        private static SingleRunInline NewDefinedRun()
        {
            var run = Run("[defined]", TriviaStyles.WhitespaceTriviaBrush);
            return new SingleRunInline(run);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(PreprocessingSymbolInfo preprocessing)
        {
            return () => GetChildren(preprocessing);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(PreprocessingSymbolInfo preprocessing)
        {
            return
            [
                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    preprocessing.Symbol,
                    Property(nameof(PreprocessingSymbolInfo.Symbol))),

                Creator.CreateRootBasic(
                    preprocessing.IsDefined,
                    Property(nameof(PreprocessingSymbolInfo.IsDefined))),
            ];
        }
    }

    public sealed class CSharpConversionRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<CSharpConversion>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            CSharpConversion conversion, GroupedRunInlineCollection inlines)
        {
            var inline = NestedTypeDisplayGroupedRun(typeof(CSharpConversion));
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.ConversionDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(CSharpConversion conversion)
        {
            return () => GetChildren(conversion);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(CSharpConversion conversion)
        {
            List<AnalysisTreeListNode> list =
            [
                Creator.CreateRootBasic(
                    conversion.Exists,
                    Property(nameof(conversion.Exists))),

                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    conversion.ConstrainedToType,
                    Property(nameof(conversion.ConstrainedToType))),

                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    conversion.MethodSymbol,
                    Property(nameof(conversion.MethodSymbol))),
            ];

            // CallerArgumentExpression would hinder performance by creating a substring
            AddPropertyNodeIfTrue(list, conversion.IsAnonymousFunction, nameof(conversion.IsAnonymousFunction));
            AddPropertyNodeIfTrue(list, conversion.IsBoxing, nameof(conversion.IsBoxing));
            AddPropertyNodeIfTrue(list, conversion.IsCollectionExpression, nameof(conversion.IsCollectionExpression));
            AddPropertyNodeIfTrue(list, conversion.IsConditionalExpression, nameof(conversion.IsConditionalExpression));
            AddPropertyNodeIfTrue(list, conversion.IsConstantExpression, nameof(conversion.IsConstantExpression));
            AddPropertyNodeIfTrue(list, conversion.IsDefaultLiteral, nameof(conversion.IsDefaultLiteral));
            AddPropertyNodeIfTrue(list, conversion.IsDynamic, nameof(conversion.IsDynamic));
            AddPropertyNodeIfTrue(list, conversion.IsEnumeration, nameof(conversion.IsEnumeration));
            AddPropertyNodeIfTrue(list, conversion.IsExplicit, nameof(conversion.IsExplicit));
            AddPropertyNodeIfTrue(list, conversion.IsIdentity, nameof(conversion.IsIdentity));
            AddPropertyNodeIfTrue(list, conversion.IsImplicit, nameof(conversion.IsImplicit));
            AddPropertyNodeIfTrue(list, conversion.IsInlineArray, nameof(conversion.IsInlineArray));
            AddPropertyNodeIfTrue(list, conversion.IsInterpolatedString, nameof(conversion.IsInterpolatedString));
            AddPropertyNodeIfTrue(list, conversion.IsInterpolatedStringHandler, nameof(conversion.IsInterpolatedStringHandler));
            AddPropertyNodeIfTrue(list, conversion.IsIntPtr, nameof(conversion.IsIntPtr));
            AddPropertyNodeIfTrue(list, conversion.IsMethodGroup, nameof(conversion.IsMethodGroup));
            AddPropertyNodeIfTrue(list, conversion.IsNullable, nameof(conversion.IsNullable));
            AddPropertyNodeIfTrue(list, conversion.IsNullLiteral, nameof(conversion.IsNullLiteral));
            AddPropertyNodeIfTrue(list, conversion.IsNumeric, nameof(conversion.IsNumeric));
            AddPropertyNodeIfTrue(list, conversion.IsObjectCreation, nameof(conversion.IsObjectCreation));
            AddPropertyNodeIfTrue(list, conversion.IsPointer, nameof(conversion.IsPointer));
            AddPropertyNodeIfTrue(list, conversion.IsReference, nameof(conversion.IsReference));
            AddPropertyNodeIfTrue(list, conversion.IsStackAlloc, nameof(conversion.IsStackAlloc));
            AddPropertyNodeIfTrue(list, conversion.IsSwitchExpression, nameof(conversion.IsSwitchExpression));
            AddPropertyNodeIfTrue(list, conversion.IsThrow, nameof(conversion.IsThrow));
            AddPropertyNodeIfTrue(list, conversion.IsTupleConversion, nameof(conversion.IsTupleConversion));
            AddPropertyNodeIfTrue(list, conversion.IsTupleLiteralConversion, nameof(conversion.IsTupleLiteralConversion));
            AddPropertyNodeIfTrue(list, conversion.IsUnboxing, nameof(conversion.IsUnboxing));
            AddPropertyNodeIfTrue(list, conversion.IsUserDefined, nameof(conversion.IsUserDefined));

            return list;
        }
    }

    public sealed class VisualBasicConversionRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<VisualBasicConversion>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            VisualBasicConversion conversion, GroupedRunInlineCollection inlines)
        {
            var inline = NestedTypeDisplayGroupedRun(typeof(VisualBasicConversion));
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.ConversionDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(VisualBasicConversion conversion)
        {
            return () => GetChildren(conversion);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(VisualBasicConversion conversion)
        {
            List<AnalysisTreeListNode> list =
            [
                Creator.CreateRootBasic(
                    conversion.Exists,
                    Property(nameof(conversion.Exists))),

                Creator.ParentContainer.SymbolCreator.CreateRootGeneral(
                    conversion.MethodSymbol,
                    Property(nameof(conversion.MethodSymbol))),
            ];

            // CallerArgumentExpression would hinder performance by creating a substring
            AddPropertyNodeIfTrue(list, conversion.IsAnonymousDelegate, nameof(conversion.IsAnonymousDelegate));
            AddPropertyNodeIfTrue(list, conversion.IsArray, nameof(conversion.IsArray));
            AddPropertyNodeIfTrue(list, conversion.IsBoolean, nameof(conversion.IsBoolean));
            AddPropertyNodeIfTrue(list, conversion.IsDefault, nameof(conversion.IsDefault));
            AddPropertyNodeIfTrue(list, conversion.IsIdentity, nameof(conversion.IsIdentity));
            AddPropertyNodeIfTrue(list, conversion.IsLambda, nameof(conversion.IsLambda));
            AddPropertyNodeIfTrue(list, conversion.IsNarrowing, nameof(conversion.IsNarrowing));
            AddPropertyNodeIfTrue(list, conversion.IsNullableValueType, nameof(conversion.IsNullableValueType));
            AddPropertyNodeIfTrue(list, conversion.IsNumeric, nameof(conversion.IsNumeric));
            AddPropertyNodeIfTrue(list, conversion.IsReference, nameof(conversion.IsReference));
            AddPropertyNodeIfTrue(list, conversion.IsString, nameof(conversion.IsString));
            AddPropertyNodeIfTrue(list, conversion.IsTypeParameter, nameof(conversion.IsTypeParameter));
            AddPropertyNodeIfTrue(list, conversion.IsUserDefined, nameof(conversion.IsUserDefined));
            AddPropertyNodeIfTrue(list, conversion.IsValueType, nameof(conversion.IsValueType));
            AddPropertyNodeIfTrue(list, conversion.IsWidening, nameof(conversion.IsWidening));

            return list;
        }
    }
}

partial class SemanticModelAnalysisNodeCreator
{
    public static SemanticModelStyles Styles
        => AppSettings.Instance.NodeColorPreferences.SemanticModelStyles!;

    public abstract class Types : CommonTypes
    {
        public const string SemanticModel = "SM";
        public const string TypeInfo = "TI";
        public const string SymbolInfo = "SI";
        public const string NullabilityInfo = "NI";
        public const string PreprocessingSymbolInfo = "PI";
        public const string Conversion = "(T)";
    }

    [SolidColor("SemanticModel", 0xFF33E5A5)]
    [SolidColor("TypeInfo", 0xFF4DCA85)]
    [SolidColor("SymbolInfo", 0xFF4DCA85)]
    [SolidColor("NullabilityInfo", 0xFF4DCA85)]
    [SolidColor("PreprocessingSymbolInfo", 0xFF4DCA85)]
    [SolidColor("Conversion", 0xFF4DCA85)]
    public sealed partial class SemanticModelStyles
    {
        public NodeTypeDisplay SemanticModelDisplay
            => new(Types.SemanticModel, SemanticModelColor);

        public NodeTypeDisplay TypeInfoDisplay
            => new(Types.TypeInfo, TypeInfoColor);

        public NodeTypeDisplay SymbolInfoDisplay
            => new(Types.SymbolInfo, SymbolInfoColor);

        public NodeTypeDisplay NullabilityInfoDisplay
            => new(Types.NullabilityInfo, NullabilityInfoColor);

        public NodeTypeDisplay PreprocessingSymbolInfoDisplay
            => new(Types.PreprocessingSymbolInfo, PreprocessingSymbolInfoColor);

        public NodeTypeDisplay ConversionDisplay
            => new(Types.Conversion, ConversionColor);
    }
}
