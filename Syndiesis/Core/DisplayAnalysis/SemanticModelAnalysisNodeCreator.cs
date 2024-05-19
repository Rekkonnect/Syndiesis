using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed partial class SemanticModelAnalysisNodeCreator(AnalysisNodeCreationOptions options)
    : BaseAnalysisNodeCreator(options)
{
    private readonly SyntaxAnalysisNodeCreator _syntaxCreator = new(options);

    private static readonly InterestingPropertyFilterCache _propertyCache
        = new(SyntaxNodePropertyFilter.Instance);

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case SemanticModel semanticModel:
                return CreateRootSemanticModel(semanticModel, valueSource);
        }

        // fallback
        return _syntaxCreator.CreateRootViewNode(value, valueSource);
    }

    private AnalysisTreeListNode CreateRootSemanticModel(
        SemanticModel semanticModel,
        DisplayValueSource valueSource)
    {
        var rootLine = CreateSemanticModelLine(semanticModel, valueSource);
        var children = GetChildRetrieverForSemanticModel(semanticModel);
        return new AnalysisTreeListNode
        {
            NodeLine = rootLine,
            ChildRetriever = children,
            // TODO: Implement an association
            AssociatedSyntaxObjectContent = null,
        };
    }

    private AnalysisTreeListNodeLine CreateSemanticModelLine(
        SemanticModel semanticModel, DisplayValueSource valueSource)
    {
        // TODO: Implement
        return null;
    }

    private AnalysisNodeChildRetriever? GetChildRetrieverForSemanticModel(SemanticModel semanticModel)
    {
        // TODO: Implement
        return null;
    }

    private IReadOnlyList<AnalysisTreeListNode> CreateSemanticModelChildren(SemanticModel semanticModel)
    {
        // TODO: Implement
        return null;
    }
}
