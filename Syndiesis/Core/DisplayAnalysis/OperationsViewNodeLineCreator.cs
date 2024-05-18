using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using System;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

using ChildRetrieverFunc = Func<IReadOnlyList<AnalysisTreeListNode>>;

public sealed partial class OperationsViewNodeLineCreator(NodeLineCreationOptions options)
    : BaseNodeLineCreator(options)
{
    private readonly SyntaxViewNodeLineCreator _syntaxCreator = new(options);

    private static readonly IOperationPropertyFilterCache _propertyCache = new();

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case IOperation operation:
                return CreateRootOperation(operation, valueSource);
        }

        // fallback
        return _syntaxCreator.CreateRootViewNode(value, valueSource);
    }

    private AnalysisTreeListNode CreateRootOperation(
        IOperation operation,
        DisplayValueSource valueSource)
    {
        var rootLine = CreateOperationLine(operation, valueSource);
        var children = GetChildRetrieverForOperation(operation);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            // TODO: Implement an association
            AssociatedSyntaxObjectContent = null,
        };
    }

    private AnalysisTreeListNodeLine CreateOperationLine(
        IOperation operation, DisplayValueSource valueSource)
    {
        // TODO: Implement
        return null;
    }

    private ChildRetrieverFunc? GetChildRetrieverForOperation(IOperation operation)
    {
        // TODO: Implement
        return null;
    }

    private IReadOnlyList<AnalysisTreeListNode> CreateOperationChildren(IOperation operation)
    {
        // TODO: Implement
        return null;
    }
}
