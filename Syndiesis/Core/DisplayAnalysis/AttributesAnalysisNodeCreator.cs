using Avalonia.Media;
using Garyon.Functions;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;

public sealed partial class AttributesAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    // node creators
    private readonly AttributeTreeRootViewNodeCreator _attributeTreeCreator;
    private readonly AttributeDataViewRootViewNodeCreator _attributeDataViewCreator;
    private readonly AttributeDataRootViewNodeCreator _attributeDataCreator;
    private readonly AttributeDataListRootViewNodeCreator _attributeDataListCreator;
    private readonly LinkedAttributeArgumentRootViewNodeCreator _linkedAttributeArgumentCreator;
    private readonly RegularLinkedAttributeArgumentListViewNodeCreator _regularLinkedAttributeArgumentListCreator;
    private readonly NamedLinkedAttributeArgumentListViewNodeCreator _namedLinkedAttributeArgumentListCreator;
    private readonly AttributeTreeSymbolContainerRootViewNodeCreator _symbolContainerCreator;

    public AttributesAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _attributeTreeCreator = new(this);
        _attributeDataViewCreator = new(this);
        _attributeDataCreator = new(this);
        _attributeDataListCreator = new(this);
        _linkedAttributeArgumentCreator = new(this);
        _regularLinkedAttributeArgumentListCreator = new(this);
        _namedLinkedAttributeArgumentListCreator = new(this);
        _symbolContainerCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case AttributeTree attributeTree:
                return CreateRootAttributeTree(attributeTree, valueSource, includeChildren);

            case AttributeTree.AttributeDataView view:
                return CreateRootAttributeView(view, valueSource, includeChildren);

            case IReadOnlyList<AttributeData> attributeList:
                return CreateRootAttributeList(attributeList, valueSource, includeChildren);

            case AttributeDataViewModel attributeDataViewModel:
                return CreateRootAttributeDataViewModel(
                    attributeDataViewModel, valueSource, includeChildren);

            case AttributeDataViewModel.LinkedAttributeArgument linkedAttributeArgument:
                return CreateRootLinkedAttributeArgument(
                    linkedAttributeArgument, valueSource, includeChildren);

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ;
    }

    public AnalysisTreeListNode CreateRootAttributeTree<TDisplayValueSource>(
        AttributeTree attributeTree,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeTreeCreator.CreateNode(attributeTree, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttributeView<TDisplayValueSource>(
        AttributeTree.AttributeDataView view,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeDataViewCreator.CreateNode(view, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttributeList<TDisplayValueSource>(
        IReadOnlyList<AttributeData> attributeList,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeDataListCreator.CreateNode(attributeList, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttributeDataViewModel<TDisplayValueSource>(
        AttributeDataViewModel attribute,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _attributeDataCreator.CreateNode(attribute, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootLinkedAttributeArgument<TDisplayValueSource>(
        AttributeDataViewModel.LinkedAttributeArgument linkedAttributeArgument,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _linkedAttributeArgumentCreator.CreateNode(
            linkedAttributeArgument, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootRegularLinkedAttributeArgumentList<TDisplayValueSource>(
        ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument> linkedAttributeArguments,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _regularLinkedAttributeArgumentListCreator.CreateNode(
            linkedAttributeArguments, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootNamedLinkedAttributeArgumentList<TDisplayValueSource>(
        ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument> linkedAttributeArguments,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _namedLinkedAttributeArgumentListCreator.CreateNode(
            linkedAttributeArguments, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootAttributeTreeSymbolContainer<TDisplayValueSource>(
        AttributeTree.SymbolContainer container,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _symbolContainerCreator.CreateNode(container, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootSyntaxNode<TDisplayValueSource>(
        SyntaxNode node,
        TDisplayValueSource? valueSource)
        where TDisplayValueSource : IDisplayValueSource
    {
        return ParentContainer.SyntaxCreator.CreateRootNode(node, valueSource, false);
    }
}

partial class AttributesAnalysisNodeCreator
{
    public abstract class AttributeRootViewNodeCreator<TValue>(AttributesAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, AttributesAnalysisNodeCreator>(creator)
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.Attribute;
        }
    }

    public sealed class AttributeTreeRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeTree>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeTree attributeTree, GroupedRunInlineCollection inlines)
        {
            var type = attributeTree.GetType();
            var inline = FullyQualifiedTypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                attributeTree.Containers.Length,
                nameof(attributeTree.Containers.Length));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.AttributeTreeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(AttributeTree tree)
        {
            if (tree.Containers.Length is 0)
                return null;

            return () => GetChildren(tree);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(AttributeTree attributeTree)
        {
            var containers = attributeTree.Containers;

            return containers
                .Select(container => Creator.CreateRootAttributeTreeSymbolContainer<IDisplayValueSource>(
                    container, default))
                .ToList()
                ;
        }
    }

    public sealed class AttributeTreeSymbolContainerRootViewNodeCreator(
        AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeTree.SymbolContainer>(creator)
    {
        public override object? AssociatedSyntaxObject(AttributeTree.SymbolContainer value)
        {
            return value.Symbol;
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeTree.SymbolContainer container, GroupedRunInlineCollection inlines)
        {
            return Creator.ParentContainer.SymbolCreator
                .CreateRootSymbolNodeLine(
                    container.Symbol, inlines)!;
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            AttributeTree.SymbolContainer container)
        {
            if (container.Attributes.Length is 0)
                return null;

            return () => GetChildren(container);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            AttributeTree.SymbolContainer container)
        {
            var attributes = container.Attributes;

            return attributes
                .Select(attribute => Creator.CreateRootGeneral<IDisplayValueSource>(attribute, default))
                .ToList()
                ;
        }
    }

    public sealed class AttributeDataViewRootViewNodeCreator(
        AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeTree.AttributeDataView>(creator)
    {
        public override object? AssociatedSyntaxObject(AttributeTree.AttributeDataView value)
        {
            return value.Data.AttributeData;
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeTree.AttributeDataView view, GroupedRunInlineCollection inlines)
        {
            var type = view.GetType();
            var inline = FullyQualifiedTypeDisplayGroupedRun(type);
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.AttributeDataViewDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            AttributeTree.AttributeDataView view)
        {
            return () => GetChildren(view);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            AttributeTree.AttributeDataView view)
        {
            var list = new List<AnalysisTreeListNode>();

            var attributeDataChild = Creator.CreateRootGeneral<IDisplayValueSource>(view.Data, default);
            list.Add(attributeDataChild);

            if (view.AttributeOperation is not null)
            {
                var attributeOperationChild = Creator.ParentContainer.OperationCreator
                    .CreateRootOperation<IDisplayValueSource>(
                        view.AttributeOperation, default);
                list.Add(attributeOperationChild);
            }

            return list;
        }
    }

    public class AttributeDataRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeDataViewModel>(creator)
    {
        public override object? AssociatedSyntaxObject(AttributeDataViewModel value)
        {
            return value.AttributeData;
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeDataViewModel attribute, GroupedRunInlineCollection inlines)
        {
            var inline = TypeDisplayGroupedRun(typeof(AttributeData));
            inlines.Add(inline);
            inlines.Add(NewValueKindSplitterRun());
            var className = attribute.AttributeData.AttributeClass?.Name;
            var nameInline = CreateNameInline(className);
            inlines.Add(nameInline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.AttributeDataDisplay);
        }

        private GroupedRunInline CreateNameInline(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return CreateNullValueSingleRun();

            return TypeDetailsInline(name);
        }

        protected GroupedRunInline TypeDetailsInline(string name)
        {
            if (name is nameof(Attribute))
            {
                return new SingleRunInline(Run(name, CommonStyles.ClassMainBrush));
            }

            var typeName = name;

            const string attributeSuffix = nameof(Attribute);
            return TypeDisplayWithFadeSuffix(
                typeName, attributeSuffix,
                CommonStyles.IdentifierWildcardBrush, CommonStyles.IdentifierWildcardFadedBrush);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            AttributeDataViewModel attribute)
        {
            return () => GetChildren(attribute);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(AttributeDataViewModel attribute)
        {
            return
            [
                Creator.ParentContainer.SymbolCreator.CreateRootSymbol(
                    attribute.AttributeData.AttributeClass!,
                    Property(nameof(AttributeData.AttributeClass)),
                    false)!,

                Creator.CreateRootRegularLinkedAttributeArgumentList(
                    attribute.ConstructorArguments,
                    Property(nameof(AttributeData.ConstructorArguments)))!,

                Creator.CreateRootNamedLinkedAttributeArgumentList(
                    attribute.NamedArguments,
                    Property(nameof(AttributeData.NamedArguments)))!,

                Creator.CreateRootGeneral(
                    attribute.AttributeData.ApplicationSyntaxReference,
                    Property(nameof(AttributeData.ApplicationSyntaxReference)))!,
            ];
        }
    }

    public class AttributeDataListRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<IReadOnlyList<AttributeData>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<AttributeData> attributes, GroupedRunInlineCollection inlines)
        {
            var type = attributes.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);
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
                .Select(attribute
                    => AttributeDataViewModel.Create(attribute, default))
                .Where(Predicates.NotNull)
                .Select(attribute
                    => Creator.CreateRootAttributeDataViewModel<IDisplayValueSource>(attribute!, default))
                .ToList()
                ;
        }
    }

    public class LinkedAttributeArgumentRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeDataViewModel.LinkedAttributeArgument>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeDataViewModel.LinkedAttributeArgument argument, GroupedRunInlineCollection inlines)
        {
            var nameBrush = BrushForAttributeArgument(argument.MappingKind);
            var nameInline = new SingleRunInline(Run(argument.Name, nameBrush));
            var splitterInline = CreateValueSplitterRun();
            var valueInline = TypeDisplayGroupedRun(typeof(TypedConstant));
            inlines.Add(nameInline);
            inlines.Add(splitterInline);
            inlines.Add(valueInline);
            inlines.Add(NewValueKindSplitterRun());
            inlines.Add(CreateKindInline(argument.Value));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.TypedConstantDisplay);
        }

        private static ILazilyUpdatedBrush BrushForAttributeArgument(
            AttributeDataViewModel.AttributeArgumentNameMappingKind kind)
        {
            return kind switch
            {
                AttributeDataViewModel.AttributeArgumentNameMappingKind.Named
                    => CommonStyles.PropertyBrush,
                AttributeDataViewModel.AttributeArgumentNameMappingKind.Parameter
                    => CommonStyles.LocalMainBrush,

                _ => CommonStyles.RawValueBrush,
            };
        } 

        public override object? AssociatedSyntaxObject(
            AttributeDataViewModel.LinkedAttributeArgument value)
        {
            return value.ArgumentSyntax;
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            AttributeDataViewModel.LinkedAttributeArgument argument)
        {
            return () => GetChildren(argument);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            AttributeDataViewModel.LinkedAttributeArgument argument)
        {
            var value = argument.Value;
            return
            [
                CreateRootTypeOrNull(
                    value.Type,
                    Property(nameof(TypedConstant.Type)))!,

                Creator.CreateRootBasic(
                    value.IsNull,
                    Property(nameof(TypedConstant.IsNull))),

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    value.Kind is not TypedConstantKind.Array,
                    () => value.Value,
                    Property(nameof(TypedConstant.Value)))!,

                Creator.CreateGeneralOrThrowsExceptionNode<InvalidOperationException>(
                    value.Kind is TypedConstantKind.Array,
                    () => value.Values,
                    Property(nameof(TypedConstant.Values)))!,
            ];
        }

        private AnalysisTreeListNode CreateRootTypeOrNull<TDisplayValueSource>(
            ITypeSymbol? type, TDisplayValueSource? valueSource)
            where TDisplayValueSource : IDisplayValueSource
        {
            if (type is null)
            {
                return Creator.CreateRootBasic(null, valueSource);
            }

            return Creator.ParentContainer.SymbolCreator.CreateRootSymbol(
                type, valueSource, false)!;
        }

        private static SingleRunInline CreateKindInline(TypedConstant constant)
        {
            return new(Run(
                constant.Kind.ToString(),
                CommonStyles.ConstantMainBrush,
                FontStyle.Italic));
        }
    }

    public abstract class LinkedAttributeArgumentListViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<
            ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument> arguments,
            GroupedRunInlineCollection inlines)
        {
            var type = GetDisplayedType();
            var inline = TypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                arguments.Length,
                nameof(ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument>.Length));

            return AnalysisTreeListNodeLine(
                inlines,
                CommonStyles.MemberAccessValueDisplay);
        }

        protected abstract Type GetDisplayedType();

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument> arguments)
        {
            if (arguments.IsEmpty())
                return null;

            return () => GetChildren(arguments);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            ImmutableArray<AttributeDataViewModel.LinkedAttributeArgument> arguments)
        {
            return arguments
                .Select(argument
                    => Creator.CreateRootLinkedAttributeArgument<IDisplayValueSource>(argument, default))
                .ToList()
                ;
        }
    }

    public class RegularLinkedAttributeArgumentListViewNodeCreator(
        AttributesAnalysisNodeCreator creator)
        : LinkedAttributeArgumentListViewNodeCreator(creator)
    {
        protected override Type GetDisplayedType()
        {
            return typeof(ImmutableArray<TypedConstant>);
        }
    }

    public class NamedLinkedAttributeArgumentListViewNodeCreator(
        AttributesAnalysisNodeCreator creator)
        : LinkedAttributeArgumentListViewNodeCreator(creator)
    {
        protected override Type GetDisplayedType()
        {
            return typeof(ImmutableArray<KeyValuePair<string, TypedConstant>>);
        }
    }
}

partial class AttributesAnalysisNodeCreator
{
    public static AttributeStyles Styles
        => AppSettings.Instance.NodeColorPreferences.AttributeStyles!;

    public abstract class Types : CommonTypes
    {
        public const string AttributeTree = "AT";
        public const string AttributeDataView = "AV";
        public const string AttributeData = "A";
        public const string AttributeDataList = "AL";

        public const string TypedConstant = "TC";
    }

    [SolidColor("AttributeData", 0xFFDE526E)]
    [SolidColor("AttributeDataList", 0xFFDE526E)]
    public sealed partial class AttributeStyles
    {
        public NodeTypeDisplay AttributeTreeDisplay
            => new(Types.AttributeTree, CommonStyles.ClassMainColor);

        public NodeTypeDisplay AttributeDataViewDisplay
            => new(Types.AttributeDataView, CommonStyles.ClassMainColor);

        public NodeTypeDisplay AttributeDataDisplay
            => new(Types.AttributeData, AttributeDataColor);

        public NodeTypeDisplay AttributeDataListDisplay
            => new(Types.AttributeDataList, AttributeDataListColor);

        public NodeTypeDisplay TypedConstantDisplay
            => new(Types.TypedConstant, CommonStyles.ConstantMainColor);
    }
}
