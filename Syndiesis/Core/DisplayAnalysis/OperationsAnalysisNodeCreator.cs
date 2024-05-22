using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using SkiaSharp;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    private readonly OperationTreeRootViewNodeCreator _operationTreeCreator;

    public OperationsAnalysisNodeCreator(
        AnalysisNodeCreationOptions options,
        AnalysisNodeCreatorContainer parentContainer)
        : base(options, parentContainer)
    {
        _operationCreator = new(this);
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
            var inline = Creator.NestedTypeDisplayGroupedRun(preferredType);
            inlines.Add(inline);

            return AnalysisTreeListNodeLine(
                inlines,
                Styles.OperationDisplay);
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

        [Obsolete("Probably useless")]
        private AnalysisTreeListNode CreateFromOperationProperty(
            PropertyInfo property,
            IOperation operation)
        {
            switch (property.Name)
            {
                case nameof(IBlockOperation.Operations):
                {
                    var node = CreateFromProperty(property, operation)
                        with
                        {
                            AssociatedSyntaxObjectContent = operation.Syntax,
                        };
                    return node;
                }
                case nameof(ILoopOperation.ChildOperations):
                {
                    var node = CreateFromProperty(property, operation)
                        with
                    {
                        AssociatedSyntaxObjectContent = operation.Syntax,
                    };
                    return node;
                }
            }

            return CreateFromProperty(property, operation);
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
    public abstract class Types : CommonTypes
    {
        public const string Operation = "O";
        public const string OperationTree = "OT";
    }

    public abstract class Styles : CommonStyles
    {
        public static readonly Color OperationColor = InterfaceMainColor;
        public static readonly SolidColorBrush OperationBrush = new(OperationColor);

        public static readonly NodeTypeDisplay OperationDisplay
            = new(Types.Operation, OperationColor);

        public static readonly NodeTypeDisplay OperationTreeDisplay
            = new(Types.OperationTree, ClassMainColor);
    }
}
