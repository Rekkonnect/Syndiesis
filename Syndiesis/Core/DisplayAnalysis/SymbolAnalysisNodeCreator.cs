﻿using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using Syndiesis.Utilities;
using System.Collections.Immutable;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;

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
    private readonly IAliasSymbolRootViewNodeCreator _aliasSymbolCreator;
    private readonly IDiscardSymbolRootViewNodeCreator _discardSymbolCreator;
    private readonly ILabelSymbolRootViewNodeCreator _labelSymbolCreator;

    private readonly SymbolListRootViewNodeCreator _symbolListCreator;
    private readonly SymbolDisplayPartRootViewNodeCreator _displayPartCreator;
    private readonly SymbolDisplayPartsListRootViewNodeCreator _displayPartsListCreator;

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
        _aliasSymbolCreator = new(this);
        _discardSymbolCreator = new(this);
        _labelSymbolCreator = new(this);

        _symbolListCreator = new(this);
        _displayPartCreator = new(this);
        _displayPartsListCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case ISymbol symbol:
                return CreateRootSymbol(symbol, valueSource, includeChildren);

            case IReadOnlyList<ISymbol> symbolList:
                return CreateRootSymbolList(symbolList, valueSource, includeChildren);

            case SymbolDisplayPart displayPart:
                return CreateRootSymbolDisplayPart(displayPart, valueSource, includeChildren);

            case ImmutableArray<SymbolDisplayPart> displayParts:
                return CreateRootSymbolDisplayPartsList(displayParts, valueSource, includeChildren);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource);
    }

    public AnalysisTreeListNodeLine? CreateRootSymbolNodeLine(
        ISymbol symbol,
        GroupedRunInlineCollection inlines)
    {
        var creator = CreatorForSymbol(symbol);
        return creator?.CreateNodeLine(symbol, inlines);
    }

    public IBaseSymbolRootViewNodeCreator? CreatorForSymbol(ISymbol symbol)
    {
        switch (symbol)
        {
            case IAssemblySymbol:
                return _assemblySymbolCreator;

            case IModuleSymbol:
                return _moduleSymbolCreator;

            case INamespaceSymbol:
                return _namespaceSymbolCreator;

            case IFieldSymbol:
                return _fieldSymbolCreator;

            case IPropertySymbol:
                return _propertySymbolCreator;

            case IEventSymbol:
                return _eventSymbolCreator;

            case IMethodSymbol:
                return _methodSymbolCreator;

            case ITypeParameterSymbol:
                return _typeParameterSymbolCreator;

            case IParameterSymbol:
                return _parameterSymbolCreator;

            case ILocalSymbol:
                return _localSymbolCreator;

            case IPreprocessingSymbol:
                return _preprocessingSymbolCreator;

            case IRangeVariableSymbol:
                return _rangeVariableSymbolCreator;

            case IAliasSymbol:
                return _aliasSymbolCreator;

            case INamedTypeSymbol:
                return _namedTypeSymbolCreator;

            case ITypeSymbol:
                return _typeSymbolCreator;

            case IDiscardSymbol:
                return _discardSymbolCreator;

            case ILabelSymbol:
                return _labelSymbolCreator;

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

            case INamedTypeSymbol namedTypeSymbol:
                return CreateRootNamedTypeSymbol(namedTypeSymbol, valueSource, includeChildren);

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

    public AnalysisTreeListNode CreateRootNamedTypeSymbol<TDisplayValueSource>(
        INamedTypeSymbol typeSymbol, TDisplayValueSource? valueSource, bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _namedTypeSymbolCreator.CreateNode(typeSymbol, valueSource, includeChildren);
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

    public AnalysisTreeListNode CreateRootSymbolDisplayPart<TDisplayValueSource>(
        SymbolDisplayPart part,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _displayPartCreator.CreateNode(part, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootSymbolDisplayPartsList<TDisplayValueSource>(
        ImmutableArray<SymbolDisplayPart> parts,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _displayPartsListCreator.CreateNode(parts, valueSource, includeChildren);
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

    public interface IBaseSymbolRootViewNodeCreator
    {
        public void AddSummaryInlines(
            ISymbol symbol, GroupedRunInlineCollection inlines);

        public AnalysisTreeListNodeLine CreateNodeLine(
            ISymbol symbol, GroupedRunInlineCollection inlines);
    }

    public abstract class ISymbolRootViewNodeCreator<TSymbol>(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<TSymbol>(creator), IBaseSymbolRootViewNodeCreator
        where TSymbol : ISymbol
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TSymbol symbol, GroupedRunInlineCollection inlines)
        {
            AddSummaryInlines(symbol, inlines);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.SymbolDisplay);
        }

        public void AddSummaryInlines(TSymbol symbol, GroupedRunInlineCollection inlines)
        {
            var type = MatchingSymbolInterface(symbol.GetType());
            var typeDetailsInline = TypeDetailsInline(type);
            inlines.Add(typeDetailsInline);
            var nameInline = CreateNameInline(symbol);
            if (nameInline is not null)
            {
                inlines.Add(CreateLargeSplitterRun());
                inlines.Add(nameInline);
            }
            inlines.Add(CreateLargeSplitterRun());
            var kindInline = CreateKindInline(symbol);
            inlines.Add(kindInline);
        }

        AnalysisTreeListNodeLine IBaseSymbolRootViewNodeCreator.CreateNodeLine(
            ISymbol symbol, GroupedRunInlineCollection inlines)
        {
            return CreateNodeLine((TSymbol)symbol, inlines);
        }

        void IBaseSymbolRootViewNodeCreator.AddSummaryInlines(
            ISymbol symbol, GroupedRunInlineCollection inlines)
        {
            AddSummaryInlines((TSymbol)symbol, inlines);
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
                Creator.ParentContainer.AttributeCreator.CreateRootAttributeList(
                    symbol.GetAttributes(),
                    MethodSource(nameof(ISymbol.GetAttributes))),

                Creator.CreateRootGeneral(
                    symbol.DeclaringSyntaxReferences,
                    Property(nameof(ISymbol.DeclaringSyntaxReferences))),

                Creator.CreateRootGeneral(
                    symbol.DeclaredAccessibility,
                    Property(nameof(ISymbol.DeclaredAccessibility))),

                Creator.CreateRootGeneral(
                    symbol.IsImplicitlyDeclared,
                    Property(nameof(ISymbol.IsImplicitlyDeclared))),

                Creator.CreateRootGeneral(
                    symbol.Name,
                    Property(nameof(ISymbol.Name))),

                Creator.CreateRootGeneral(
                    symbol.MetadataName,
                    Property(nameof(ISymbol.MetadataName))),

                Creator.CreateRootGeneral(
                    symbol.ToDisplayString(),
                    MethodSource(nameof(ISymbol.ToDisplayString))),

                Creator.CreateRootGeneral(
                    symbol.ToDisplayParts(),
                    MethodSource(nameof(ISymbol.ToDisplayParts))),
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

        protected override void CreateChildren(
            TSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.AddRange([
                Creator.CreateRootGeneral(
                    symbol.IsExtension,
                    Property(nameof(ITypeSymbol.IsExtension))),

                Creator.CreateRootGeneral(
                    symbol.ExtensionParameter,
                    Property(nameof(ITypeSymbol.ExtensionParameter))),
            ]);

            base.CreateChildren(symbol, list);
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

                Creator.CreateRootSymbolList(
                    symbol.TypeArguments,
                    Property(nameof(INamedTypeSymbol.TypeArguments))),

                Creator.CreateRootGeneral(
                    symbol.IsFileLocal,
                    Property(nameof(INamedTypeSymbol.IsFileLocal))),

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
                    Property(nameof(IPropertySymbol.RefKind))),

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
                    Property(nameof(IMethodSymbol.CallingConvention))),

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

                Creator.ParentContainer.AttributeCreator.CreateRootAttributeList(
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
                    Property(nameof(ITypeParameterSymbol.Variance)))
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
                    Property(nameof(IParameterSymbol.RefKind))),

                Creator.CreateRootGeneral(
                    symbol.ScopedKind,
                    Property(nameof(IParameterSymbol.ScopedKind))),

                Creator.CreateRootSymbol(
                    symbol.Type,
                    Property(nameof(IParameterSymbol.Type)),
                    false)!,

                // ExplicitDefaultValue could be an `Optional<object?>`,
                // but is kept as originally designed
                // Keep this in mind in the event that this changes in the future
                Creator.CreateRootGeneral(
                    symbol.HasExplicitDefaultValue,
                    Property(nameof(IParameterSymbol.HasExplicitDefaultValue))),

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    symbol.HasExplicitDefaultValue,
                    () => symbol.ExplicitDefaultValue,
                    Property(nameof(IParameterSymbol.ExplicitDefaultValue))),
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

    public sealed class IAliasSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IAliasSymbol>(creator)
    {
        protected override void CreateChildren(
            IAliasSymbol symbol, List<AnalysisTreeListNode> list)
        {
            list.Add(
                Creator.CreateRootSymbol(
                    symbol.Target,
                    Property(nameof(IAliasSymbol.Target)))!
            );
        }
    }

    public sealed class IDiscardSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<IDiscardSymbol>(creator)
    {
    }

    public sealed class ILabelSymbolRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : ISymbolRootViewNodeCreator<ILabelSymbol>(creator)
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

    public sealed class SymbolDisplayPartsListRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<ImmutableArray<SymbolDisplayPart>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            ImmutableArray<SymbolDisplayPart> displayParts, GroupedRunInlineCollection inlines)
        {
            AppendDisplayPartColors(displayParts, inlines);
            return AnalysisTreeListNodeLine(
                inlines,
                Styles.DisplayPartsDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            ImmutableArray<SymbolDisplayPart> parts)
        {
            if (parts.IsEmpty())
                return null;

            return () => GetChildren(parts);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            ImmutableArray<SymbolDisplayPart> parts)
        {
            return parts
                .Select(part => Creator.CreateRootSymbolDisplayPart<IDisplayValueSource>(part, default))
                .ToList()
                ;
        }

        private void AppendDisplayPartColors(
            ImmutableArray<SymbolDisplayPart> displayParts,
            GroupedRunInlineCollection inlines)
        {
            inlines.AddRange(
                displayParts
                    .Select(SymbolDisplayPartRootViewNodeCreator.CreateDisplayPartInline));
        }
    }

    public sealed class SymbolDisplayPartRootViewNodeCreator(SymbolAnalysisNodeCreator creator)
        : SymbolRootViewNodeCreator<SymbolDisplayPart>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SymbolDisplayPart part, GroupedRunInlineCollection inlines)
        {
            var inline = CreateDisplayPartInline(part);
            inlines.Add(inline);
            var splitter = CreateLargeSplitterRun();
            inlines.Add(splitter);
            var kindRun = EnumRootAnalysisNodeCreator.EnumValueRun(part.Kind);
            inlines.Add(kindRun);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.DisplayPartsDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            SymbolDisplayPart value)
        {
            return null;
        }

        public static SingleRunInline CreateDisplayPartInline(SymbolDisplayPart part)
        {
            var brush = BrushForDisplayPart(part.Kind);
            return new(new(part.ToString(), brush));
        }

        public static ILazilyUpdatedBrush BrushForDisplayPart(SymbolDisplayPartKind kind)
        {
            return kind switch
            {
                // TODO: Introduce a color for aliases
                SymbolDisplayPartKind.AliasName => CommonStyles.RawValueBrush,
                // TODO: Introduce a color for assemblies
                SymbolDisplayPartKind.AssemblyName => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.ClassName => ColorizationStyles.ClassBrush,
                SymbolDisplayPartKind.DelegateName => ColorizationStyles.DelegateBrush,
                SymbolDisplayPartKind.EnumName => ColorizationStyles.EnumFieldBrush,
                // TODO: Introduce a color for error types
                SymbolDisplayPartKind.ErrorTypeName => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.EventName => ColorizationStyles.EventBrush,
                SymbolDisplayPartKind.FieldName => ColorizationStyles.FieldBrush,
                SymbolDisplayPartKind.InterfaceName => ColorizationStyles.InterfaceBrush,
                SymbolDisplayPartKind.Keyword => ColorizationStyles.KeywordBrush,
                SymbolDisplayPartKind.LabelName => ColorizationStyles.LabelBrush,
                SymbolDisplayPartKind.LineBreak => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.NumericLiteral => ColorizationStyles.NumericLiteralBrush,
                SymbolDisplayPartKind.StringLiteral => ColorizationStyles.StringLiteralBrush,
                SymbolDisplayPartKind.LocalName => ColorizationStyles.LocalBrush,
                SymbolDisplayPartKind.MethodName => ColorizationStyles.MethodBrush,
                SymbolDisplayPartKind.ModuleName => ColorizationStyles.ModuleBrush,
                // TODO: Introduce a color for namespaces
                SymbolDisplayPartKind.NamespaceName => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.Operator => ColorizationStyles.MethodBrush,
                SymbolDisplayPartKind.ParameterName => ColorizationStyles.ParameterBrush,
                SymbolDisplayPartKind.PropertyName => ColorizationStyles.PropertyBrush,
                SymbolDisplayPartKind.Punctuation => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.Space => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.StructName => ColorizationStyles.StructBrush,
                SymbolDisplayPartKind.AnonymousTypeIndicator => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.Text => CommonStyles.RawValueBrush,
                SymbolDisplayPartKind.TypeParameterName => ColorizationStyles.TypeParameterBrush,
                SymbolDisplayPartKind.RangeVariableName => ColorizationStyles.RangeVariableBrush,
                SymbolDisplayPartKind.EnumMemberName => ColorizationStyles.EnumFieldBrush,
                SymbolDisplayPartKind.ExtensionMethodName => ColorizationStyles.MethodBrush,
                SymbolDisplayPartKind.ConstantName => ColorizationStyles.ConstantBrush,
                // TODO: Introduce a color for record class types
                SymbolDisplayPartKind.RecordClassName => ColorizationStyles.ClassBrush,
                // TODO: Introduce a color for record struct types
                SymbolDisplayPartKind.RecordStructName => ColorizationStyles.StructBrush,

                _ => CommonStyles.RawValueBrush,
            };
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

        public const string DisplayParts = "P";
    }

    [SolidColor("Symbol", 0xFFA2D080)]
    [SolidColor("SymbolCollection", 0xFF4DCA85)]
    [SolidColor("DisplayParts", 0xFF63A8A8)]
    public sealed partial class SymbolStyles
    {
        public NodeTypeDisplay SymbolDisplay
            => new(Types.Symbol, SymbolColor);

        public NodeTypeDisplay SymbolCollectionDisplay
            => new(Types.SymbolCollection, SymbolCollectionColor);

        public NodeTypeDisplay DisplayPartsDisplay
            => new(Types.DisplayParts, DisplayPartsColor);
    }
}
