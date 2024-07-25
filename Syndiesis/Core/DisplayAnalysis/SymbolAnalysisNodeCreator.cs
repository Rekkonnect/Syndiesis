using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
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
    private readonly INamedTypeSymbolRootViewNodeCreator _namedTypeSymbolCreator;
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
        _namedTypeSymbolCreator = new(this);
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

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case ISymbol symbol:
                return CreateRootSymbol(symbol, valueSource, includeChildren);

            case IReadOnlyList<ISymbol> symbolList:
                return CreateRootSymbolList(symbolList, valueSource, includeChildren);

            case AttributeData attribute:
                return CreateRootAttribute(attribute, valueSource, includeChildren);

            case IReadOnlyList<AttributeData> attributeList:
                return CreateRootAttributeList(attributeList, valueSource, includeChildren);

            case TypedConstant typedConstant:
                return CreateRootTypedConstant(typedConstant, valueSource, includeChildren);

            default:
                break;
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource);
    }

    public AnalysisTreeListNodeLine? CreateRootSymbolNodeLine(
        ISymbol symbol,
        GroupedRunInlineCollection inlines)
    {
        switch (symbol)
        {
            case IAssemblySymbol assemblySymbol:
                return _assemblySymbolCreator.CreateNodeLine(assemblySymbol, inlines);

            case IModuleSymbol moduleSymbol:
                return _moduleSymbolCreator.CreateNodeLine(moduleSymbol, inlines);

            case INamespaceSymbol namespaceSymbol:
                return _namespaceSymbolCreator.CreateNodeLine(namespaceSymbol, inlines);

            case IFieldSymbol fieldSymbol:
                return _fieldSymbolCreator.CreateNodeLine(fieldSymbol, inlines);

            case IPropertySymbol propertySymbol:
                return _propertySymbolCreator.CreateNodeLine(propertySymbol, inlines);

            case IEventSymbol eventSymbol:
                return _eventSymbolCreator.CreateNodeLine(eventSymbol, inlines);

            case IMethodSymbol methodSymbol:
                return _methodSymbolCreator.CreateNodeLine(methodSymbol, inlines);

            case ITypeParameterSymbol typeParameter:
                return _typeParameterSymbolCreator.CreateNodeLine(typeParameter, inlines);

            case IParameterSymbol parameter:
                return _parameterSymbolCreator.CreateNodeLine(parameter, inlines);

            case ILocalSymbol localSymbol:
                return _localSymbolCreator.CreateNodeLine(localSymbol, inlines);

            case IPreprocessingSymbol preprocessingSymbol:
                return _preprocessingSymbolCreator.CreateNodeLine(preprocessingSymbol, inlines);

            case IRangeVariableSymbol rangeVariableSymbol:
                return _rangeVariableSymbolCreator.CreateNodeLine(rangeVariableSymbol, inlines);

            case INamedTypeSymbol namedTypeSymbol:
                return _namedTypeSymbolCreator.CreateNodeLine(namedTypeSymbol, inlines);

            case ITypeSymbol typeSymbol:
                return _typeSymbolCreator.CreateNodeLine(typeSymbol, inlines);

            default:
                return null;
        }
    }

    public AnalysisTreeListNode CreateRootSymbol<TDisplayValueSource>(
        ISymbol symbol,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        switch (symbol)
        {
            case IAssemblySymbol assemblySymbol:
                return CreateRootAssemblySymbol(assemblySymbol, valueSource, includeChildren);

            case IModuleSymbol moduleSymbol:
                return CreateRootModuleSymbol(moduleSymbol, valueSource, includeChildren);

            case INamespaceSymbol namespaceSymbol:
                return CreateRootNamespaceSymbol(namespaceSymbol, valueSource, includeChildren);

            case IFieldSymbol fieldSymbol:
                return CreateRootFieldSymbol(fieldSymbol, valueSource, includeChildren);

            case IPropertySymbol propertySymbol:
                return CreateRootPropertySymbol(propertySymbol, valueSource, includeChildren);

            case IEventSymbol eventSymbol:
                return CreateRootEventSymbol(eventSymbol, valueSource, includeChildren);

            case IMethodSymbol methodSymbol:
                return CreateRootMethodSymbol(methodSymbol, valueSource, includeChildren);

            case ITypeParameterSymbol typeParameter:
                return CreateRootTypeParameterSymbol(typeParameter, valueSource, includeChildren);

            case IParameterSymbol parameter:
                return CreateRootParameterSymbol(parameter, valueSource, includeChildren);

            case ILocalSymbol localSymbol:
                return CreateRootLocalSymbol(localSymbol, valueSource, includeChildren);

            case IPreprocessingSymbol preprocessingSymbol:
                return CreateRootPreprocessingSymbol(preprocessingSymbol, valueSource, includeChildren);

            case IRangeVariableSymbol rangeVariableSymbol:
                return CreateRootRangeVariableSymbol(rangeVariableSymbol, valueSource, includeChildren);

            case ITypeSymbol typeSymbol:
                return CreateRootTypeSymbol(typeSymbol, valueSource, includeChildren);

            default:
                return CreateRootSymbolFallback(symbol, valueSource, includeChildren);
        }
    }

    public AnalysisTreeListNode CreateRootAssemblySymbol<TDisplayValueSource>(
        IAssemblySymbol assemblySymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _assemblySymbolCreator.CreateNode(assemblySymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootModuleSymbol<TDisplayValueSource>(
        IModuleSymbol moduleSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _moduleSymbolCreator.CreateNode(moduleSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootNamespaceSymbol<TDisplayValueSource>(
        INamespaceSymbol namespaceSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _namespaceSymbolCreator.CreateNode(namespaceSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootTypeSymbol<TDisplayValueSource>(
        ITypeSymbol typeSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _typeSymbolCreator.CreateNode(typeSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootFieldSymbol<TDisplayValueSource>(
        IFieldSymbol fieldSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _fieldSymbolCreator.CreateNode(fieldSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootPropertySymbol<TDisplayValueSource>(
        IPropertySymbol propertySymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _propertySymbolCreator.CreateNode(propertySymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootEventSymbol<TDisplayValueSource>(
        IEventSymbol eventSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _eventSymbolCreator.CreateNode(eventSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootMethodSymbol<TDisplayValueSource>(
        IMethodSymbol methodSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _methodSymbolCreator.CreateNode(methodSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootTypeParameterSymbol<TDisplayValueSource>(
        ITypeParameterSymbol typeParameter, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _typeParameterSymbolCreator.CreateNode(typeParameter, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootParameterSymbol<TDisplayValueSource>(
        IParameterSymbol parameterSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _parameterSymbolCreator.CreateNode(parameterSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootLocalSymbol<TDisplayValueSource>(
        ILocalSymbol localSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _localSymbolCreator.CreateNode(localSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootPreprocessingSymbol<TDisplayValueSource>(
        IPreprocessingSymbol preprocessingSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _preprocessingSymbolCreator.CreateNode(preprocessingSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootRangeVariableSymbol<TDisplayValueSource>(
        IRangeVariableSymbol rangeVariableSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _rangeVariableSymbolCreator.CreateNode(rangeVariableSymbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootSymbolFallback<TDisplayValueSource>(
        ISymbol symbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _symbolCreator.CreateNode(symbol, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootSymbolList<TDisplayValueSource>(
        IReadOnlyList<ISymbol> symbols,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _symbolListCreator.CreateNode(symbols, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttribute<TDisplayValueSource>(
        AttributeData attribute,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeDataCreator.CreateNode(attribute, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttributeList<TDisplayValueSource>(
        IReadOnlyList<AttributeData> attributeList,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeDataListCreator.CreateNode(attributeList, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootTypedConstant<TDisplayValueSource>(
        TypedConstant typedConstant,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _typedConstantCreator.CreateNode(typedConstant, valueSource, includeChildren);
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
            TSymbol symbol, GroupedRunInlineCollection inlines)
        {
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
            var kindInline = CreateKindInline(symbol);
            inlines.Add(kindInline);

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
            return CreateKindInlineForText(symbol.Kind.ToString());
        }

        protected static SingleRunInline CreateKindInlineForText(string text)
        {
            return new(Run(
                text,
                CommonStyles.ConstantMainBrush,
                FontStyle.Italic));
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
            return CreateKindInlineForText(symbol.TypeKind.ToString());
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
            list.AddRange([
                Creator.CreateRootSymbolList(
                    symbol.TypeParameters,
                    Property(nameof(INamedTypeSymbol.TypeParameters))),

                Creator.CreateRootGeneral(
                    symbol.DelegateInvokeMethod,
                    Property(nameof(INamedTypeSymbol.DelegateInvokeMethod))),
            ]);

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

                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(IFieldSymbol.Type)),
                    false)!,

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

                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(IPropertySymbol.Type)),
                    false)!,

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
                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(IEventSymbol.Type)),
                    false)!
            );

            base.CreateChildren(symbol, list);
        }
    }

    public sealed class IMethodSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IMethodSymbol>(creator)
    {
        protected override SingleRunInline CreateKindInline(IMethodSymbol symbol)
        {
            return CreateKindInlineForText(symbol.MethodKind.ToString());
        }

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

                Creator.CreateRootSymbol(
                    symbol.ReturnType,
                    Property(nameof(IMethodSymbol.ReturnType)),
                    false)!,

                Creator.CreateRootSymbolList(
                    symbol.TypeParameters,
                    Property(nameof(IMethodSymbol.TypeParameters))),

                Creator.CreateRootSymbolList(
                    symbol.Parameters,
                    Property(nameof(IMethodSymbol.Parameters))),

                Creator.CreateRootAttributeList(
                    symbol.GetReturnTypeAttributes(),
                    MethodSource(nameof(IMethodSymbol.GetReturnTypeAttributes))),
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

                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(IParameterSymbol.Type)),
                    false)!,

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

                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(ILocalSymbol.Type)),
                    false)!,

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

    // Congratulations, you were proven worthy
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
            IReadOnlyList<ISymbol> symbols, GroupedRunInlineCollection inlines)
        {
            var type = symbols.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);
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
                .Select(symbol => Creator.CreateRootSymbol<IDisplayValueSource>(symbol, default))
                .ToList()
                ;
        }
    }

    public sealed class AttributeDataRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : AttributesAnalysisNodeCreator.AttributeDataRootViewNodeCreator(
            creator.ParentContainer.AttributeCreator)
    {
        public override AnalysisNodeKind GetNodeKind(AttributeData value)
        {
            return AnalysisNodeKind.Symbol;
        }
    }

    public sealed class AttributeDataListRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : AttributesAnalysisNodeCreator.AttributeDataListRootViewNodeCreator(
            creator.ParentContainer.AttributeCreator)
    {
        public override AnalysisNodeKind GetNodeKind(IReadOnlyList<AttributeData> value)
        {
            return AnalysisNodeKind.Symbol;
        }
    }

    public sealed class TypedConstantRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : AttributesAnalysisNodeCreator.TypedConstantRootViewNodeCreator(
            creator.ParentContainer.AttributeCreator)
    {
        public override AnalysisNodeKind GetNodeKind(TypedConstant value)
        {
            return AnalysisNodeKind.Symbol;
        }
    }
}

partial class SymbolAnalysisNodeCreator
{
    public static SymbolStyles Styles
        => AppSettings.Instance.NodeColorPreferences.SymbolStyles!;

    public abstract class Types : CommonTypes
    {
        public const string Symbol = "S";

        // 'Collection' was used instead of 'List' to avoid
        // the confusion of SL with [Separated]SyntaxList
        public const string SymbolCollection = "SC";
    }

    [SolidColor("Symbol", 0xFFA2D080)]
    [SolidColor("SymbolCollection", 0xFF4DCA85)]
    public sealed partial class SymbolStyles
    {
        public NodeTypeDisplay SymbolDisplay
            => new(Types.Symbol, SymbolColor);

        public NodeTypeDisplay SymbolCollectionDisplay
            => new(Types.SymbolCollection, SymbolCollectionColor);
    }
}
