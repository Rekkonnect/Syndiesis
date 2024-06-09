using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;
using static Syndiesis.Core.DisplayAnalysis.SymbolAnalysisNodeCreator;

public sealed partial class AttributesAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    // node creators
    private readonly AttributeTreeRootViewNodeCreator _attributeTreeCreator;
    private readonly AttributeDataRootViewNodeCreator _attributeDataCreator;
    private readonly AttributeDataListRootViewNodeCreator _attributeDataListCreator;
    private readonly AttributeTreeSymbolContainerRootViewNodeCreator _symbolContainerCreator;

    public AttributesAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _attributeTreeCreator = new(this);
        _attributeDataCreator = new(this);
        _attributeDataListCreator = new(this);
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
            AttributeTree AttributeTree, DisplayValueSource valueSource)
        {
            var type = AttributeTree.GetType();
            var inline = FullyQualifiedTypeDisplayGroupedRun(type);

            return AnalysisTreeListNodeLine(
                [inline],
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
            var type = attribute.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);
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
                    attribute.ConstructorArguments,
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
}

partial class AttributesAnalysisNodeCreator
{
    public static AttributeStyles Styles
        => AppSettings.Instance.StylePreferences.AttributeStyles!;

    public abstract class Types : CommonTypes
    {
        public const string AttributeTree = "AT";
        public const string AttributeData = "A";
        public const string AttributeDataList = "AL";
    }

    public class AttributeStyles
    {
        public Color AttributeDataColor = Color.FromUInt32(0xFFDE526E);
        public Color AttributeDataListColor = Color.FromUInt32(0xFFDE526E);

        public NodeTypeDisplay AttributeTreeDisplay
            => new(Types.AttributeTree, CommonStyles.ClassMainColor);

        public NodeTypeDisplay AttributeDataDisplay
            => new(Types.AttributeData, AttributeDataColor);

        public NodeTypeDisplay AttributeDataListDisplay
            => new(Types.AttributeDataList, AttributeDataListColor);

    }
}
