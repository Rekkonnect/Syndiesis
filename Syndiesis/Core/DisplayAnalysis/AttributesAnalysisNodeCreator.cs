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

public sealed partial class AttributesAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    // node creators
    private readonly AttributeTreeRootViewNodeCreator _attributeTreeCreator;
    private readonly AttributeDataRootViewNodeCreator _attributeDataCreator;
    private readonly AttributeDataListRootViewNodeCreator _attributeDataListCreator;
    private readonly TypedConstantRootViewNodeCreator _typedConstantCreator;
    private readonly AttributeTreeSymbolContainerRootViewNodeCreator _symbolContainerCreator;

    public AttributesAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _attributeTreeCreator = new(this);
        _attributeDataCreator = new(this);
        _attributeDataListCreator = new(this);
        _typedConstantCreator = new(this);
        _symbolContainerCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case AttributeTree AttributeTree:
                return CreateRootAttributeTree(AttributeTree, valueSource);

            case AttributeData attribute:
                return CreateRootAttribute(attribute, valueSource);

            case IReadOnlyList<AttributeData> attributeList:
                return CreateRootAttributeList(attributeList, valueSource);

            case TypedConstant typedConstant:
                return CreateRootTypedConstant(typedConstant, valueSource);

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ;
    }

    public AnalysisTreeListNode CreateRootAttributeTree(
        AttributeTree AttributeTree,
        DisplayValueSource valueSource)
    {
        return _attributeTreeCreator.CreateNode(AttributeTree, valueSource);
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

    public AnalysisTreeListNode CreateRootAttributeTreeSymbolContainer(
        AttributeTree.SymbolContainer container,
        DisplayValueSource valueSource)
    {
        return _symbolContainerCreator.CreateNode(container, valueSource);
    }

    public AnalysisTreeListNode CreateRootSyntaxNode(
        SyntaxNode node,
        DisplayValueSource valueSource)
    {
        return ParentContainer.SyntaxCreator.CreateChildlessRootNode(node, valueSource);
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
            AttributeTree attributeTree, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
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

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(AttributeTree AttributeTree)
        {
            var containers = AttributeTree.Containers;

            return containers
                .Select(container => Creator.CreateRootAttributeTreeSymbolContainer(
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
            AttributeTree.SymbolContainer container, DisplayValueSource valueSource)
        {
            return Creator.ParentContainer.SymbolCreator
                .CreateRootSymbolNodeLine(
                    container.Symbol, valueSource)!;
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
                .Select(attribute => Creator.CreateRootGeneral(attribute, default))
                .ToList()
                ;
        }
    }

    public class AttributeDataRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<AttributeData>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            AttributeData attribute, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            var inline = TypeDisplayGroupedRun(typeof(AttributeData));
            inlines.Add(inline);
            inlines.Add(NewValueKindSplitterRun());
            var className = attribute.AttributeClass?.Name;
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
            AttributeData attribute)
        {
            return () => GetChildren(attribute);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(AttributeData attribute)
        {
            return
            [
                Creator.ParentContainer.SymbolCreator.CreateRootChildlessSymbol(
                    attribute.AttributeClass!,
                    Property(nameof(AttributeData.AttributeClass)))!,

                Creator.CreateRootGeneral(
                    attribute.ConstructorArguments,
                    Property(nameof(AttributeData.ConstructorArguments)))!,

                Creator.CreateRootGeneral(
                    attribute.NamedArguments,
                    Property(nameof(AttributeData.NamedArguments)))!,

                Creator.CreateRootGeneral(
                    attribute.ApplicationSyntaxReference,
                    Property(nameof(AttributeData.ApplicationSyntaxReference)))!,
            ];
        }
    }

    public class AttributeDataListRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<IReadOnlyList<AttributeData>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<AttributeData> attributes, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
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
                .Select(symbol => Creator.CreateRootAttribute(symbol, default))
                .ToList()
                ;
        }
    }

    public class TypedConstantRootViewNodeCreator(AttributesAnalysisNodeCreator creator)
        : AttributeRootViewNodeCreator<TypedConstant>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            TypedConstant constant, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            var inline = NestedTypeDisplayGroupedRun(typeof(TypedConstant));
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
                CreateRootTypeOrNull(
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

        private AnalysisTreeListNode CreateRootTypeOrNull(
            ITypeSymbol? type, DisplayValueSource valueSource)
        {
            if (type is null)
            {
                return Creator.CreateRootBasic(null, valueSource);
            }

            return Creator.ParentContainer.SymbolCreator.CreateRootChildlessSymbol(
                type, valueSource)!;
        }

        private static SingleRunInline CreateKindInline(TypedConstant constant)
        {
            return new(Run(constant.Kind.ToString(), CommonStyles.ConstantMainBrush));
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

        public NodeTypeDisplay AttributeDataDisplay
            => new(Types.AttributeData, AttributeDataColor);

        public NodeTypeDisplay AttributeDataListDisplay
            => new(Types.AttributeDataList, AttributeDataListColor);

        public NodeTypeDisplay TypedConstantDisplay
            => new(Types.TypedConstant, CommonStyles.ConstantMainColor);
    }
}
