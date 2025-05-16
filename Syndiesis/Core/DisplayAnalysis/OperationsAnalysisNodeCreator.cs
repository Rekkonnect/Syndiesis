using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using Syndiesis.InternalGenerators.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;
using GroupedRunInline = GroupedRunInline.IBuilder;
// This also captures IOperation.OperationList
using OperationList = IReadOnlyCollection<IOperation>;
using SingleRunInline = SingleRunInline.Builder;

public sealed partial class OperationsAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    private static readonly IOperationPropertyFilterCache _propertyCache = new();

    // node creators
    private readonly OperationRootViewNodeCreator _operationCreator;
    private readonly OperationListRootViewNodeCreator _operationListCreator;
    private readonly OperationTreeRootViewNodeCreator _operationTreeCreator;
    private readonly OperationTreeSymbolContainerRootViewNodeCreator _symbolContainerCreator;

    public OperationsAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _operationCreator = new(this);
        _operationListCreator = new(this);
        _operationTreeCreator = new(this);
        _symbolContainerCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode<TDisplayValueSource>(
        object? value, TDisplayValueSource? valueSource, bool includeChildren)
        where TDisplayValueSource : default
    {
        switch (value)
        {
            case IOperation operation:
                return CreateRootOperation(operation, valueSource, includeChildren);

            case OperationList operationList:
                return CreateRootOperationList(operationList, valueSource, includeChildren);

            case OperationTree operationTree:
                return CreateRootOperationTree(operationTree, valueSource, includeChildren);

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource, includeChildren)
            ?? ParentContainer.SymbolCreator.CreateRootViewNode(value, valueSource, includeChildren)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource, includeChildren)
            ;
    }

    public AnalysisTreeListNode CreateRootOperation<TDisplayValueSource>(
        IOperation operation,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _operationCreator.CreateNode(operation, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootOperationList<TDisplayValueSource>(
        OperationList operations,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _operationListCreator.CreateNode(operations, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootOperationTree<TDisplayValueSource>(
        OperationTree operationTree,
        TDisplayValueSource? valueSource,
        bool includeChildren = true)
        where TDisplayValueSource : IDisplayValueSource
    {
        return _operationTreeCreator.CreateNode(operationTree, valueSource, includeChildren);
    }

    public AnalysisTreeListNode CreateRootOperationTreeSymbolContainer<TDisplayValueSource>(
        OperationTree.SymbolContainer container,
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

partial class OperationsAnalysisNodeCreator
{
    public abstract class OperationRootViewNodeCreator<TValue>(OperationsAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, OperationsAnalysisNodeCreator>(creator)
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.Operation;
        }
    }

    public sealed class OperationRootViewNodeCreator(OperationsAnalysisNodeCreator creator)
        : OperationRootViewNodeCreator<IOperation>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IOperation operation, GroupedRunInlineCollection inlines)
        {
            var type = operation.GetType();
            var preferredType = _propertyCache.FilterForType(type).PreferredType ?? type;
            var typeDetailsInline = TypeDetailsInline(preferredType);
            inlines.Add(typeDetailsInline);
            inlines.Add(CreateLargeSplitterRun());
            var kindInline = CreateKindInline(operation.Kind);
            inlines.Add(kindInline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.OperationDisplay);
        }

        private GroupedRunInline TypeDetailsInline(Type type)
        {
            if (type == typeof(IOperation))
            {
                return new SingleRunInline(Run(type.Name, CommonStyles.InterfaceMainBrush));
            }

            var typeName = type.Name;

            const string operationSuffix = "Operation";
            return TypeDisplayWithFadeSuffix(
                typeName, operationSuffix,
                CommonStyles.InterfaceMainBrush, CommonStyles.InterfaceSecondaryBrush);
        }

        private SingleRunInline CreateKindInline(OperationKind kind)
        {
            return new(Run(
                kind.ToString(),
                CommonStyles.ConstantMainBrush,
                FontStyle.Italic));
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(IOperation operation)
        {
            return () => GetChildren(operation);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(IOperation operation)
        {
            var type = _propertyCache.FilterForType(operation.GetType());
            var properties = type.Properties;
            var preferredType = type.PreferredType;

            return properties
                .OrderBy(s => s.Name)
                .Select(property => CreateFromPropertyWithSyntaxObject(property, operation))
                .ToList()
                ;
        }
    }

    public sealed class OperationListRootViewNodeCreator(OperationsAnalysisNodeCreator creator)
        : OperationRootViewNodeCreator<OperationList>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            OperationList operations, GroupedRunInlineCollection inlines)
        {
            var type = operations.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                operations.Count,
                nameof(OperationList.Count));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.OperationListDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            OperationList operations)
        {
            if (operations.Count is 0)
                return null;

            return () => GetChildren(operations);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            OperationList operations)
        {
            return operations
                .Select(operation => Creator.CreateRootOperation<IDisplayValueSource>(operation, default))
                .ToList()
                ;
        }
    }

    public sealed class OperationTreeRootViewNodeCreator(OperationsAnalysisNodeCreator creator)
        : OperationRootViewNodeCreator<OperationTree>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            OperationTree operationTree, GroupedRunInlineCollection inlines)
        {
            var type = operationTree.GetType();
            var inline = FullyQualifiedTypeDisplayGroupedRun(type);
            inlines.Add(inline);
            AppendCountValueDisplay(
                inlines,
                operationTree.Containers.Length,
                nameof(operationTree.Containers.Length));

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.OperationTreeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(OperationTree operationTree)
        {
            if (operationTree.Containers.Length is 0)
                return null;

            return () => GetChildren(operationTree);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(OperationTree operationTree)
        {
            var containers = operationTree.Containers;

            return containers
                .Select(container => Creator.CreateRootOperationTreeSymbolContainer<IDisplayValueSource>(
                    container, default))
                .ToList()
                ;
        }
    }

    public sealed class OperationTreeSymbolContainerRootViewNodeCreator(
        OperationsAnalysisNodeCreator creator)
        : OperationRootViewNodeCreator<OperationTree.SymbolContainer>(creator)
    {
        public override object? AssociatedSyntaxObject(OperationTree.SymbolContainer value)
        {
            return value.Symbol;
        }

        public override AnalysisTreeListNodeLine CreateNodeLine(
            OperationTree.SymbolContainer container, GroupedRunInlineCollection inlines)
        {
            return Creator.ParentContainer.SymbolCreator
                .CreateRootSymbolNodeLine(
                    container.Symbol, inlines)!;
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            OperationTree.SymbolContainer container)
        {
            Debug.Assert(container.Operations.Length > 0);
            return () => GetChildren(container);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            OperationTree.SymbolContainer container)
        {
            var operations = container.Operations;

            return operations
                .Select(operation => Creator.CreateRootOperation<IDisplayValueSource>(operation, default))
                .ToList()
                ;
        }
    }
}

partial class OperationsAnalysisNodeCreator
{
    public static OperationStyles Styles
        => AppSettings.Instance.NodeColorPreferences.OperationStyles!;

    public abstract class Types : CommonTypes
    {
        public const string Operation = "O";
        public const string OperationList = "OL";
        public const string OperationTree = "OT";
    }

    [SolidColor("Operation", 0xFFA2D080)]
    public sealed partial class OperationStyles
    {
        public NodeTypeDisplay OperationDisplay
            => new(Types.Operation, OperationColor);

        public NodeTypeDisplay OperationListDisplay
            => new(Types.OperationList, CommonStyles.StructMainColor);

        public NodeTypeDisplay OperationTreeDisplay
            => new(Types.OperationTree, CommonStyles.ClassMainColor);
    }
}
