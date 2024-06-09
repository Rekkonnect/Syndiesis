using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
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

// This also captures IOperation.OperationList
using OperationList = IReadOnlyCollection<IOperation>;

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

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case IOperation operation:
                return CreateRootOperation(operation, valueSource);

            case OperationList operationList:
                return CreateRootOperationList(operationList, valueSource);

            case OperationTree operationTree:
                return CreateRootOperationTree(operationTree, valueSource);

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SymbolCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource)
            ;
    }

    public AnalysisTreeListNode CreateRootOperation(
        IOperation operation,
        DisplayValueSource valueSource)
    {
        return _operationCreator.CreateNode(operation, valueSource);
    }

    public AnalysisTreeListNode CreateRootOperationList(
        OperationList operations,
        DisplayValueSource valueSource)
    {
        return _operationListCreator.CreateNode(operations, valueSource);
    }

    public AnalysisTreeListNode CreateRootOperationTree(
        OperationTree operationTree,
        DisplayValueSource valueSource)
    {
        return _operationTreeCreator.CreateNode(operationTree, valueSource);
    }

    public AnalysisTreeListNode CreateRootOperationTreeSymbolContainer(
        OperationTree.SymbolContainer container,
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
            IOperation operation, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
            var type = operation.GetType();
            var preferredType = _propertyCache.FilterForType(type).PreferredType ?? type;
            var typeDetailsInline = TypeDetailsInline(preferredType);
            inlines.Add(typeDetailsInline);
            inlines.Add(NewValueKindSplitterRun());
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
            return new(Run(kind.ToString(), CommonStyles.ConstantMainBrush));
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
            OperationList operations, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            AppendValueSource(valueSource, inlines);
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
                .Select(operation => Creator.CreateRootOperation(operation, default))
                .ToList()
                ;
        }
    }

    public sealed class OperationTreeRootViewNodeCreator(OperationsAnalysisNodeCreator creator)
        : OperationRootViewNodeCreator<OperationTree>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            OperationTree operationTree, DisplayValueSource valueSource)
        {
            var type = operationTree.GetType();
            var inline = FullyQualifiedTypeDisplayGroupedRun(type);

            return AnalysisTreeListNodeLine(
                [inline],
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
                .Select(container => Creator.CreateRootOperationTreeSymbolContainer(
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
            OperationTree.SymbolContainer container, DisplayValueSource valueSource)
        {
            return Creator.ParentContainer.SymbolCreator
                .CreateRootSymbolNodeLine(
                    container.Symbol, valueSource)!;
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
                .Select(operation => Creator.CreateRootOperation(operation, default))
                .ToList()
                ;
        }
    }
}

partial class OperationsAnalysisNodeCreator
{
    public static OperationStyles Styles
        => AppSettings.Instance.StylePreferences.OperationStyles!;

    public abstract class Types : CommonTypes
    {
        public const string Operation = "O";
        public const string OperationList = "OL";
        public const string OperationTree = "OT";
    }

    public class OperationStyles
    {
        public Color OperationColor = CommonStyles.InterfaceMainColor;

        public NodeTypeDisplay OperationDisplay
            => new(Types.Operation, OperationColor);

        public NodeTypeDisplay OperationListDisplay
            => new(Types.OperationList, CommonStyles.StructMainColor);

        public NodeTypeDisplay OperationTreeDisplay
            => new(Types.OperationTree, CommonStyles.ClassMainColor);
    }
}
