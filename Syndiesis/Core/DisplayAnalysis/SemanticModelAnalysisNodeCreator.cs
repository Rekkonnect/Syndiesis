using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

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
            // TODO: Implement
            //case SemanticModel semanticModel:
            //    return CreateRootSemanticModel(semanticModel, valueSource);
            default:
                break;
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
        return AnalysisTreeListNode(
            rootLine,
            children,
            null
        );
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
