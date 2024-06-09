using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
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

public sealed partial class AttributesAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    // node creators
    private readonly AttributeTreeRootViewNodeCreator _attributeTreeCreator;
    private readonly AttributeTreeSymbolContainerRootViewNodeCreator _symbolContainerCreator;

    public AttributesAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _attributeTreeCreator = new(this);
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

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SymbolCreator.CreateRootViewNode(value, valueSource)
            ;
    }

    public AnalysisTreeListNode CreateRootAttributeTree(
        AttributeTree AttributeTree,
        DisplayValueSource valueSource)
    {
        return _attributeTreeCreator.CreateNode(AttributeTree, valueSource);
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
            var inline = Creator.NestedTypeDisplayGroupedRun(type);

            return AnalysisTreeListNodeLine(
                [inline],
                Styles.AttributeTreeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(AttributeTree AttributeTree)
        {
            if (AttributeTree.Containers.Length is 0)
                return null;

            return () => GetChildren(AttributeTree);
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
            Debug.Assert(container.Attributes.Length > 0);
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
}

partial class AttributesAnalysisNodeCreator
{
    public static AttributeStyles Styles
        => AppSettings.Instance.StylePreferences.AttributeStyles!;

    public abstract class Types : CommonTypes
    {
        public const string AttributeTree = "AT";
    }

    public class AttributeStyles
    {
        public NodeTypeDisplay AttributeTreeDisplay
            => new(Types.AttributeTree, CommonStyles.ClassMainColor);
    }
}
