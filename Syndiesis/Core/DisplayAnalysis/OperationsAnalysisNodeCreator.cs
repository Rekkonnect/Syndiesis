using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
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

public sealed partial class OperationsAnalysisNodeCreator
    : BaseAnalysisNodeCreator
{
    private static readonly IOperationPropertyFilterCache _propertyCache = new();

    // node creators
    private readonly OperationRootViewNodeCreator _operationCreator;
    private readonly OperationListRootViewNodeCreator _operationListCreator;
    private readonly OperationTreeRootViewNodeCreator _operationTreeCreator;

    public OperationsAnalysisNodeCreator(
        AnalysisNodeCreationOptions options,
        AnalysisNodeCreatorContainer parentContainer)
        : base(options, parentContainer)
    {
        _operationCreator = new(this);
        _operationListCreator = new(this);
        _operationTreeCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case IOperation operation:
                return CreateRootOperation(operation, valueSource);

            case IReadOnlyList<IOperation> operationList:
                return CreateRootOperationList(operationList, valueSource);

            case OperationTree operationTree:
                return CreateRootOperationTree(operationTree, valueSource);

            case SyntaxNode syntaxNode:
                return CreateRootSyntaxNode(syntaxNode, valueSource);
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SemanticCreator.CreateRootViewNode(value, valueSource)
            ?? ParentContainer.SymbolCreator.CreateRootViewNode(value, valueSource)
            ;
    }

    public AnalysisTreeListNode CreateRootOperation(
        IOperation operation,
        DisplayValueSource valueSource)
    {
        return _operationCreator.CreateNode(operation, valueSource);
    }

    public AnalysisTreeListNode CreateRootOperationList(
        IReadOnlyList<IOperation> operations,
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

    public AnalysisTreeListNode CreateRootSyntaxNode(
        SyntaxNode node,
        DisplayValueSource valueSource)
    {
        return ParentContainer.SyntaxCreator.ChildlessSyntaxCreator
            .CreateNode(node, valueSource);
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
            Creator.AppendValueSource(valueSource, inlines);
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
        : OperationRootViewNodeCreator<IReadOnlyList<IOperation>>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            IReadOnlyList<IOperation> operations, DisplayValueSource valueSource)
        {
            var inlines = new GroupedRunInlineCollection();
            Creator.AppendValueSource(valueSource, inlines);
            var type = operations.GetType();
            var inline = Creator.NestedTypeDisplayGroupedRun(type);
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.OperationListDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(
            IReadOnlyList<IOperation> operations)
        {
            if (operations.Count is 0)
                return null;

            return () => GetChildren(operations);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(
            IReadOnlyList<IOperation> operations)
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
            var inline = Creator.NestedTypeDisplayGroupedRun(type);

            return AnalysisTreeListNodeLine(
                [inline],
                Styles.OperationTreeDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(OperationTree operationTree)
        {
            if (operationTree.Operations.Length is 0)
                return null;

            return () => GetChildren(operationTree);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(OperationTree operationTree)
        {
            var operations = operationTree.Operations;

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
