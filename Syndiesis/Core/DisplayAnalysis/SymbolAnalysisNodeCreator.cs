using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

public sealed partial class SymbolAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    // node creators
    private readonly ISymbolRootViewNodeCreator _symbolCreator;
    private readonly IAssemblySymbolRootViewNodeCreator _assemblySymbolCreator;
    private readonly IModuleSymbolRootViewNodeCreator _moduleSymbolCreator;
    private readonly INamespaceSymbolRootViewNodeCreator _namespaceSymbolCreator;
    private readonly ITypeSymbolRootViewNodeCreator _typeSymbolCreator;
    private readonly IFieldSymbolRootViewNodeCreator _fieldSymbolCreator;
    private readonly IPropertySymbolRootViewNodeCreator _propertySymbolCreator;
    private readonly IEventSymbolRootViewNodeCreator _eventSymbolCreator;
    private readonly IMethodSymbolRootViewNodeCreator _methodSymbolCreator;
    private readonly ITypeParameterSymbolRootViewNodeCreator _typeParameterSymbolCreator;
    private readonly IParameterSymbolRootViewNodeCreator _parameterSymbolCreator;
    private readonly ILocalSymbolRootViewNodeCreator _localSymbolCreator;
    private readonly IPreprocessingSymbolRootViewNodeCreator _preprocessingSymbolCreator;
    private readonly IRangeVariableSymbolRootViewNodeCreator _rangeVariableSymbolCreator;

    private readonly SymbolListRootViewNodeCreator _symbolListCreator;
    private readonly AttributeDataRootViewNodeCreator _attributeDataCreator;
    private readonly AttributeDataListRootViewNodeCreator _attributeDataListCreator;
    private readonly TypedConstantRootViewNodeCreator _typedConstantCreator;

    public SymbolAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _symbolCreator = new(this);
        _assemblySymbolCreator = new(this);
        _moduleSymbolCreator = new(this);
        _namespaceSymbolCreator = new(this);
        _typeSymbolCreator = new(this);
        _fieldSymbolCreator = new(this);
        _propertySymbolCreator = new(this);
        _eventSymbolCreator = new(this);
        _methodSymbolCreator = new(this);
        _typeParameterSymbolCreator = new(this);
        _parameterSymbolCreator = new(this);
        _localSymbolCreator = new(this);
        _preprocessingSymbolCreator = new(this);
        _rangeVariableSymbolCreator = new(this);

        _symbolListCreator = new(this);
        _attributeDataCreator = new(this);
        _attributeDataListCreator = new(this);
        _typedConstantCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case ISymbol symbol:
                return CreateRootSymbol(symbol, valueSource);

            case IReadOnlyList<ISymbol> symbolList:
                return CreateRootSymbolList(symbolList, valueSource);

            case AttributeData attribute:
                return CreateRootAttribute(attribute, valueSource);

            case IReadOnlyList<AttributeData> attributeList:
                return CreateRootAttributeList(attributeList, valueSource);

            case TypedConstant typedConstant:
                return CreateRootTypedConstant(typedConstant, valueSource);

            default:
                break;
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource);
    }

    public AnalysisTreeListNodeLine? CreateRootSymbolNodeLine(
        ISymbol symbol,
        DisplayValueSource valueSource)
    {
        switch (symbol)
        {
            case IAssemblySymbol assemblySymbol:
                return _assemblySymbolCreator.CreateNodeLine(assemblySymbol, valueSource);

            case IModuleSymbol moduleSymbol:
                return _moduleSymbolCreator.CreateNodeLine(moduleSymbol, valueSource);

            case INamespaceSymbol namespaceSymbol:
                return _namespaceSymbolCreator.CreateNodeLine(namespaceSymbol, valueSource);

            case IFieldSymbol fieldSymbol:
                return _fieldSymbolCreator.CreateNodeLine(fieldSymbol, valueSource);

            case IPropertySymbol propertySymbol:
                return _propertySymbolCreator.CreateNodeLine(propertySymbol, valueSource);

            case IEventSymbol eventSymbol:
                return _eventSymbolCreator.CreateNodeLine(eventSymbol, valueSource);

            case IMethodSymbol methodSymbol:
                return _methodSymbolCreator.CreateNodeLine(methodSymbol, valueSource);

            case ITypeParameterSymbol typeParameter:
                return _typeParameterSymbolCreator.CreateNodeLine(typeParameter, valueSource);

            case IParameterSymbol parameter:
                return _parameterSymbolCreator.CreateNodeLine(parameter, valueSource);

            case ILocalSymbol localSymbol:
                return _localSymbolCreator.CreateNodeLine(localSymbol, valueSource);

            case IPreprocessingSymbol preprocessingSymbol:
                return _preprocessingSymbolCreator.CreateNodeLine(preprocessingSymbol, valueSource);

            case IRangeVariableSymbol rangeVariableSymbol:
                return _rangeVariableSymbolCreator.CreateNodeLine(rangeVariableSymbol, valueSource);

            case ITypeSymbol typeSymbol:
                return _typeSymbolCreator.CreateNodeLine(typeSymbol, valueSource);

            default:
                return null;
        }
    }

    public AnalysisTreeListNode? CreateRootChildlessSymbol(
        ISymbol symbol,
        DisplayValueSource valueSource)
    {
        switch (symbol)
        {
            case IAssemblySymbol assemblySymbol:
                return _assemblySymbolCreator.CreateChildlessNode(assemblySymbol, valueSource);

            case IModuleSymbol moduleSymbol:
                return _moduleSymbolCreator.CreateChildlessNode(moduleSymbol, valueSource);

            case INamespaceSymbol namespaceSymbol:
                return _namespaceSymbolCreator.CreateChildlessNode(namespaceSymbol, valueSource);

            case IFieldSymbol fieldSymbol:
                return _fieldSymbolCreator.CreateChildlessNode(fieldSymbol, valueSource);

            case IPropertySymbol propertySymbol:
                return _propertySymbolCreator.CreateChildlessNode(propertySymbol, valueSource);

            case IEventSymbol eventSymbol:
                return _eventSymbolCreator.CreateChildlessNode(eventSymbol, valueSource);

            case IMethodSymbol methodSymbol:
                return _methodSymbolCreator.CreateChildlessNode(methodSymbol, valueSource);

            case ITypeParameterSymbol typeParameter:
                return _typeParameterSymbolCreator.CreateChildlessNode(typeParameter, valueSource);

            case IParameterSymbol parameter:
                return _parameterSymbolCreator.CreateChildlessNode(parameter, valueSource);

            case ILocalSymbol localSymbol:
                return _localSymbolCreator.CreateChildlessNode(localSymbol, valueSource);

            case IPreprocessingSymbol preprocessingSymbol:
                return _preprocessingSymbolCreator.CreateChildlessNode(preprocessingSymbol, valueSource);

            case IRangeVariableSymbol rangeVariableSymbol:
                return _rangeVariableSymbolCreator.CreateChildlessNode(rangeVariableSymbol, valueSource);

            case ITypeSymbol typeSymbol:
                return _typeSymbolCreator.CreateChildlessNode(typeSymbol, valueSource);
        }

        return null;
    }

    public AnalysisTreeListNode CreateRootSymbol(
        ISymbol symbol,
        DisplayValueSource valueSource)
    {
        switch (symbol)
        {
            case IAssemblySymbol assemblySymbol:
                return CreateRootAssemblySymbol(assemblySymbol, valueSource);

            case IModuleSymbol moduleSymbol:
                return CreateRootModuleSymbol(moduleSymbol, valueSource);

            case INamespaceSymbol namespaceSymbol:
                return CreateRootNamespaceSymbol(namespaceSymbol, valueSource);

            case IFieldSymbol fieldSymbol:
                return CreateRootFieldSymbol(fieldSymbol, valueSource);

            case IPropertySymbol propertySymbol:
                return CreateRootPropertySymbol(propertySymbol, valueSource);

            case IEventSymbol eventSymbol:
                return CreateRootEventSymbol(eventSymbol, valueSource);

            case IMethodSymbol methodSymbol:
                return CreateRootMethodSymbol(methodSymbol, valueSource);

            case ITypeParameterSymbol typeParameter:
                return CreateRootTypeParameterSymbol(typeParameter, valueSource);

            case IParameterSymbol parameter:
                return CreateRootParameterSymbol(parameter, valueSource);

            case ILocalSymbol localSymbol:
                return CreateRootLocalSymbol(localSymbol, valueSource);

            case IPreprocessingSymbol preprocessingSymbol:
                return CreateRootPreprocessingSymbol(preprocessingSymbol, valueSource);

            case IRangeVariableSymbol rangeVariableSymbol:
                return CreateRootRangeVariableSymbol(rangeVariableSymbol, valueSource);

            case ITypeSymbol typeSymbol:
                return CreateRootTypeSymbol(typeSymbol, valueSource);

            default:
                return CreateRootSymbolFallback(symbol, valueSource);
        }
    }

    public AnalysisTreeListNode CreateRootAssemblySymbol(
        IAssemblySymbol assemblySymbol, DisplayValueSource valueSource)
    {
        return _assemblySymbolCreator.CreateNode(assemblySymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootModuleSymbol(
        IModuleSymbol moduleSymbol, DisplayValueSource valueSource)
    {
        return _moduleSymbolCreator.CreateNode(moduleSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootNamespaceSymbol(
        INamespaceSymbol namespaceSymbol, DisplayValueSource valueSource)
    {
        return _namespaceSymbolCreator.CreateNode(namespaceSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootTypeSymbol(
        ITypeSymbol typeSymbol, DisplayValueSource valueSource)
    {
        return _typeSymbolCreator.CreateNode(typeSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootFieldSymbol(
        IFieldSymbol fieldSymbol, DisplayValueSource valueSource)
    {
        return _fieldSymbolCreator.CreateNode(fieldSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootPropertySymbol(
        IPropertySymbol propertySymbol, DisplayValueSource valueSource)
    {
        return _propertySymbolCreator.CreateNode(propertySymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootEventSymbol(
        IEventSymbol eventSymbol, DisplayValueSource valueSource)
    {
        return _eventSymbolCreator.CreateNode(eventSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootMethodSymbol(
        IMethodSymbol methodSymbol, DisplayValueSource valueSource)
    {
        return _methodSymbolCreator.CreateNode(methodSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootTypeParameterSymbol(
        ITypeParameterSymbol typeParameter, DisplayValueSource valueSource)
    {
        return _typeParameterSymbolCreator.CreateNode(typeParameter, valueSource);
    }

    public AnalysisTreeListNode CreateRootParameterSymbol(
        IParameterSymbol parameterSymbol, DisplayValueSource valueSource)
    {
        return _parameterSymbolCreator.CreateNode(parameterSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootLocalSymbol(
        ILocalSymbol localSymbol, DisplayValueSource valueSource)
    {
        return _localSymbolCreator.CreateNode(localSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootPreprocessingSymbol(
        IPreprocessingSymbol preprocessingSymbol, DisplayValueSource valueSource)
    {
        return _preprocessingSymbolCreator.CreateNode(preprocessingSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootRangeVariableSymbol(
        IRangeVariableSymbol rangeVariableSymbol, DisplayValueSource valueSource)
    {
        return _rangeVariableSymbolCreator.CreateNode(rangeVariableSymbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootSymbolFallback(
        ISymbol symbol, DisplayValueSource valueSource)
    {
        return _symbolCreator.CreateNode(symbol, valueSource);
    }

    public AnalysisTreeListNode CreateRootSymbolList(
        IReadOnlyList<ISymbol> symbols,
        DisplayValueSource valueSource)
    {
        return _symbolListCreator.CreateNode(symbols, valueSource);
    }

    public AnalysisTreeListNode CreateRootAttribute(
        AttributeData attribute,
        DisplayValueSource valueSource)
    {
        return _attributeDataCreator.CreateNode(attribute, valueSource);
    }

    public AnalysisTreeListNode CreateRootAttributeList(
        IReadOnlyList<AttributeData> attributeList,
        DisplayValueSource valueSource)
    {
        return _attributeDataListCreator.CreateNode(attributeList, valueSource);
    }

    public AnalysisTreeListNode CreateRootTypedConstant(
        TypedConstant typedConstant,
        DisplayValueSource valueSource)
    {
        return _typedConstantCreator.CreateNode(typedConstant, valueSource);
    }
}

partial class SymbolAnalysisNodeCreator
{
    public abstract class SymbolRootViewNodeCreator<TValue>(SymbolAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, SymbolAnalysisNodeCreator>(creator)
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.Symbol;
        }

        protected Type MatchingSymbolInterface(Type type)
        {
            var directInterfaces = type.GetInterfaceInheritanceTree()
                .Root.Children.Select(s => s.Value);

            // We want to get a single symbol interface from the
            // type passed onto the caller
            return directInterfaces.Single(IsSymbolInterface)!;

            static bool IsSymbolInterface(Type type)
            {
                return type.IsOrImplements(typeof(ISymbol));
            }
        }

        protected GroupedRunInline TypeDetailsInline(Type type)
        {
            if (type == typeof(ISymbol))
            {
                return new SingleRunInline(Run(type.Name, CommonStyles.InterfaceMainBrush));
            }

            var typeName = type.Name;

            const string symbolSuffix = "Symbol";
            return TypeDisplayWithFadeSuffix(
                typeName, symbolSuffix,
                CommonStyles.InterfaceMainBrush, CommonStyles.InterfaceSecondaryBrush);
        }
    }

    public abstract class ISymbolRootViewNodeCreator<TSymbol>(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<TSymbol>(creator)
        where TSymbol : ISymbol
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TSymbol symbol, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var type = MatchingSymbolInterface(symbol.GetType());
            var typeDetailsInline = TypeDetailsInline(type);
            inlines.Add(typeDetailsInline);
            var nameInline = CreateNameInline(symbol);
            if (nameInline is not null)
            {
                inlines.Add(NewValueKindSplitterRun());
                inlines.Add(nameInline);
            }
            inlines.Add(NewValueKindSplitterRun());
            inlines.Add(CreateKindInline(symbol));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SymbolDisplay);
        }

        protected virtual SingleRunInline? CreateNameInline(TSymbol symbol)
        {
            if (string.IsNullOrEmpty(symbol.Name))
                return null;

            return new(Run(symbol.Name, CommonStyles.IdentifierWildcardBrush));
        }

        protected virtual SingleRunInline CreateKindInline(TSymbol symbol)
        {
            return new(Run(symbol.Kind.ToString(), CommonStyles.ConstantMainBrush));
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(TSymbol symbol)
        {
            return () => GetChildren(symbol);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(TSymbol symbol)
        {
            var list = new List<AnalysisTreeListNode>();
            CreateChildren(symbol, list);
            return list;
        }

        protected virtual void CreateChildren(
            TSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange(
            [
                Creator.CreateRootAttributeList(
                    symbol.GetAttributes(),
                    MethodSource(nameof(ISymbol.GetAttributes))),

                Creator.CreateRootGeneral(
                    symbol.DeclaringSyntaxReferences,
                    Property(nameof(ISymbol.DeclaringSyntaxReferences)))!,

                Creator.CreateRootGeneral(
                    symbol.DeclaredAccessibility,
                    Property(nameof(ISymbol.DeclaredAccessibility)))!,

                Creator.CreateRootGeneral(
                    symbol.IsImplicitlyDeclared,
                    Property(nameof(ISymbol.IsImplicitlyDeclared)))!,

                Creator.CreateRootGeneral(
                    symbol.Name,
                    Property(nameof(ISymbol.Name)))!,

                Creator.CreateRootGeneral(
                    symbol.MetadataName,
                    Property(nameof(ISymbol.MetadataName)))!,

                Creator.CreateRootGeneral(
                    symbol.ToDisplayString(),
                    MethodSource(nameof(ISymbol.ToDisplayString)))!,
            ]);
        }
    }

    public sealed class ISymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<ISymbol>(creator)
    {
    }

    public sealed class IAssemblySymbolRootViewNodeCreator(
        SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IAssemblySymbol>(creator)
    {
        public override object? AssociatedSyntaxObject(IAssemblySymbol value)
        {
            return value.GlobalNamespace;
        }

        protected override void CreateChildren(IAssemblySymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootSymbol(
                    symbol.GlobalNamespace,
                    Property(nameof(IAssemblySymbol.GlobalNamespace))),
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IModuleSymbolRootViewNodeCreator(
        SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IModuleSymbol>(creator)
    {
        protected override void CreateChildren(IModuleSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootSymbol(
                    symbol.GlobalNamespace,
                    Property(nameof(IModuleSymbol.GlobalNamespace)))
            );

            base.CreateChildren(symbol, list);
        }
    }

    public abstract class INamespaceOrTypeSymbolRootViewNodeCreator<TSymbol>(
        SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<TSymbol>(creator)
        where TSymbol : INamespaceOrTypeSymbol
    {
        protected override void CreateChildren(
            TSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootSymbolList(
                    symbol.GetMembers(),
                    MethodSource(nameof(INamespaceOrTypeSymbol.GetMembers)))
            );

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class INamespaceSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : INamespaceOrTypeSymbolRootViewNodeCreator<INamespaceSymbol>(creator)
    {
        protected override void CreateChildren(
            INamespaceSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootGeneral(
                    symbol.NamespaceKind,
                    Property(nameof(INamespaceSymbol.NamespaceKind)))!
            );

            base.CreateChildren(symbol, list);
        }

        protected override SingleRunInline CreateNameInline(INamespaceSymbol symbol)
        {
            if (symbol.IsGlobalNamespace)
            {
                return new(Run("<global-namespace>", CommonStyles.IdentifierWildcardFadedBrush));
            }

            return base.CreateNameInline(symbol)!;
        }
    }

    public abstract class ITypeSymbolRootViewNodeCreator<TSymbol>(SymbolAnalysisNodeCreator creator)
        : INamespaceOrTypeSymbolRootViewNodeCreator<TSymbol>(creator)
        where TSymbol : ITypeSymbol
    {
        protected override SingleRunInline CreateKindInline(TSymbol symbol)
        {
            return new(Run(symbol.TypeKind.ToString(), CommonStyles.ConstantMainBrush));
        }
    }

    public sealed class ITypeSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ITypeSymbolRootViewNodeCreator<ITypeSymbol>(creator)
    {
    }

    public sealed class INamedTypeSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ITypeSymbolRootViewNodeCreator<INamedTypeSymbol>(creator)
    {
        protected override void CreateChildren(
            INamedTypeSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootSymbolList(
                    symbol.TypeParameters,
                    Property(nameof(INamedTypeSymbol.TypeParameters)))
            );

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IFieldSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IFieldSymbol>(creator)
    {
        protected override void CreateChildren(
            IFieldSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.RefKind,
                    Property(nameof(IFieldSymbol.RefKind)))!,

                Creator.CreateRootChildlessSymbol(
                    symbol.Type,
                    Property(nameof(IFieldSymbol.Type)))!,

                // ConstantValue could be an `Optional<object?>`,
                // but is kept as originally designed
                // Keep this in mind in the event that this changes in the future
                Creator.CreateRootGeneral(
                    symbol.HasConstantValue,
                    Property(nameof(IFieldSymbol.HasConstantValue)))!,

                Creator.CreateRootGeneral(
                    symbol.ConstantValue,
                    Property(nameof(IFieldSymbol.ConstantValue)))!,
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IPropertySymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IPropertySymbol>(creator)
    {
        protected override void CreateChildren(
            IPropertySymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.RefKind,
                    Property(nameof(IPropertySymbol.RefKind)))!,

                Creator.CreateRootChildlessSymbol(
                    symbol.Type,
                    Property(nameof(IPropertySymbol.Type)))!,

                Creator.CreateRootSymbolList(
                    symbol.Parameters,
                    Property(nameof(IPropertySymbol.Parameters))),
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IEventSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IEventSymbol>(creator)
    {
        protected override void CreateChildren(
            IEventSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootChildlessSymbol(
                    symbol.Type,
                    Property(nameof(IEventSymbol.Type)))!
            );

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IMethodSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IMethodSymbol>(creator)
    {
        protected override void CreateChildren(
            IMethodSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.CallingConvention,
                    Property(nameof(IMethodSymbol.CallingConvention)))!,

                Creator.CreateRootGeneral(
                    symbol.RefKind,
                    Property(nameof(IMethodSymbol.RefKind)))!,

                Creator.CreateRootChildlessSymbol(
                    symbol.ReturnType,
                    Property(nameof(IMethodSymbol.ReturnType)))!,

                Creator.CreateRootSymbolList(
                    symbol.TypeParameters,
                    Property(nameof(IMethodSymbol.TypeParameters))),

                Creator.CreateRootSymbolList(
                    symbol.Parameters,
                    Property(nameof(IMethodSymbol.Parameters))),
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class ITypeParameterSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ITypeSymbolRootViewNodeCreator<ITypeParameterSymbol>(creator)
    {
        protected override void CreateChildren(
            ITypeParameterSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootGeneral(
                    symbol.Variance,
                    Property(nameof(ITypeParameterSymbol.Variance)))!
            );

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IParameterSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IParameterSymbol>(creator)
    {
        protected override void CreateChildren(
            IParameterSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.RefKind,
                    Property(nameof(IParameterSymbol.RefKind)))!,

                Creator.CreateRootGeneral(
                    symbol.ScopedKind,
                    Property(nameof(IParameterSymbol.ScopedKind)))!,

                Creator.CreateRootChildlessSymbol(
                    symbol.Type,
                    Property(nameof(IParameterSymbol.Type)))!,

                // ExplicitDefaultValue could be an `Optional<object?>`,
                // but is kept as originally designed
                // Keep this in mind in the event that this changes in the future
                Creator.CreateRootGeneral(
                    symbol.HasExplicitDefaultValue,
                    Property(nameof(IParameterSymbol.HasExplicitDefaultValue)))!,

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    symbol.HasExplicitDefaultValue,
                    () => symbol.ExplicitDefaultValue,
                    Property(nameof(IParameterSymbol.ExplicitDefaultValue)))!,
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class ILocalSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<ILocalSymbol>(creator)
    {
        protected override SingleRunInline CreateNameInline(ILocalSymbol symbol)
        {
            return new(Run(symbol.Name, CommonStyles.LocalMainBrush));
        }

        protected override void CreateChildren(
            ILocalSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.RefKind,
                    Property(nameof(ILocalSymbol.RefKind)))!,

                Creator.CreateRootGeneral(
                    symbol.ScopedKind,
                    Property(nameof(ILocalSymbol.ScopedKind)))!,

                Creator.CreateRootChildlessSymbol(
                    symbol.Type,
                    Property(nameof(ILocalSymbol.Type)))!,

                // ConstantValue could be an `Optional<object?>`,
                // but is kept as originally designed
                // Keep this in mind in the event that this changes in the future
                Creator.CreateRootGeneral(
                    symbol.HasConstantValue,
                    Property(nameof(ILocalSymbol.HasConstantValue)))!,

                Creator.CreateRootGeneral(
                    symbol.ConstantValue,
                    Property(nameof(ILocalSymbol.ConstantValue)))!,
            ]);

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IPreprocessingSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IPreprocessingSymbol>(creator)
    {
        protected override void CreateChildren(
            IPreprocessingSymbol symbol, List<AnalysisTreeListNode> list)
        {
            // Preprocessing symbols are very boring

            list.Add(
                Creator.CreateRootGeneral(
                    symbol.Name,
                    Property(nameof(IPreprocessingSymbol.Name)))!
            );
        }
    }

    public sealed class IRangeVariableSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IRangeVariableSymbol>(creator)
    {
    }

    public sealed class SymbolListRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<IReadOnlyList<ISymbol>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<ISymbol> symbols, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var type = symbols.GetType();
            var inline = Creator.NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                symbols.Count,
                nameof(IReadOnlyList<ISymbol>.Count));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SymbolCollectionDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            IReadOnlyList<ISymbol> symbols)
        {
            if (symbols.IsEmpty())
                return null;

            return () => GetChildren(symbols);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            IReadOnlyList<ISymbol> symbols)
        {
            return symbols
                .Select(symbol => Creator.CreateRootSymbol(symbol, default))
                .ToList()
                ;
        }
    }

    public sealed class AttributeDataRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<AttributeData>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeData attribute, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var type = attribute.GetType();
            var inline = Creator.NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.AttributeDataDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            AttributeData attribute)
        {
            return () => GetChildren(attribute);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(AttributeData attribute)
        {
            return
            [
                Creator.CreateRootChildlessSymbol(
                    attribute.AttributeClass!,
                    Property(nameof(AttributeData.AttributeClass)))!,

                Creator.CreateRootGeneral(
                    attribute.ConstructorArguments,
                    Property(nameof(AttributeData.ConstructorArguments)))!,

                Creator.CreateRootGeneral(
                    attribute.NamedArguments,
                    Property(nameof(AttributeData.NamedArguments)))!,

                Creator.CreateRootGeneral(
                    attribute.ConstructorArguments,
                    Property(nameof(AttributeData.ApplicationSyntaxReference)))!,
            ];
        }
    }

    public sealed class AttributeDataListRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<IReadOnlyList<AttributeData>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<AttributeData> attributes, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var type = attributes.GetType();
            var inline = Creator.NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                attributes.Count,
                nameof(IReadOnlyList<AttributeData>.Count));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.AttributeDataListDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            IReadOnlyList<AttributeData> attributes)
        {
            if (attributes.IsEmpty())
                return null;

            return () => GetChildren(attributes);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            IReadOnlyList<AttributeData> attributes)
        {
            return attributes
                .Select(symbol => Creator.CreateRootAttribute(symbol, default))
                .ToList()
                ;
        }
    }

    public sealed class TypedConstantRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<TypedConstant>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TypedConstant constant, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var inline = Creator.NestedTypeDisplayGroupedRun(typeof(TypedConstant));
            inlines.Add(inline);
            inlines.Add(NewValueKindSplitterRun());
            inlines.Add(CreateKindInline(constant));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TypedConstantDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            TypedConstant constant)
        {
            return () => GetChildren(constant);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(TypedConstant constant)
        {
            return
            [
                Creator.CreateRootGeneral(
                    constant.Type,
                    Property(nameof(TypedConstant.Type)))!,

                Creator.CreateRootBasic(
                    constant.IsNull,
                    Property(nameof(TypedConstant.IsNull))),

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    constant.Kind is not TypedConstantKind.Array,
                    () => constant.Value,
                    Property(nameof(TypedConstant.Value)))!,

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    constant.Kind is TypedConstantKind.Array,
                    () => constant.Values,
                    Property(nameof(TypedConstant.Values)))!,
            ];
        }

        private static SingleRunInline CreateKindInline(TypedConstant constant)
        {
            return new(Run(constant.Kind.ToString(), CommonStyles.ConstantMainBrush));
        }
    }
}

partial class SymbolAnalysisNodeCreator
{
    public static SymbolStyles Styles
        => AppSettings.Instance.StylePreferences.SymbolStyles!;

    public abstract class Types : CommonTypes
    {
        public const string Symbol = "S";

        // 'Collection' was used instead of 'List' to avoid
        // the confusion of SL with [Separated]SyntaxList
        public const string SymbolCollection = "SC";

        public const string AttributeData = "A";
        public const string AttributeDataList = "AL";
        public const string TypedConstant = "TC";
    }

    public class SymbolStyles
    {
        public Color SymbolColor = CommonStyles.InterfaceMainColor;
        public Color SymbolCollectionColor = CommonStyles.StructMainColor;
        public Color AttributeDataColor = Color.FromUInt32(0xFFDE526E);
        public Color AttributeDataListColor = Color.FromUInt32(0xFFDE526E);

        public NodeTypeDisplay SymbolDisplay
            => new(Types.Symbol, SymbolColor);

        public NodeTypeDisplay SymbolCollectionDisplay
            => new(Types.SymbolCollection, SymbolCollectionColor);

        public NodeTypeDisplay AttributeDataDisplay
            => new(Types.AttributeData, AttributeDataColor);

        public NodeTypeDisplay AttributeDataListDisplay
            => new(Types.AttributeDataList, AttributeDataListColor);

        public NodeTypeDisplay TypedConstantDisplay
            => new(Types.TypedConstant, CommonStyles.ConstantMainColor);
    }
}
