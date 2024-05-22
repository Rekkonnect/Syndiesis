using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

public sealed partial class SymbolAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    private static readonly ISymbolPropertyFilterCache _propertyCache = new();

    public SymbolAnalysisNodeCreator(
        AnalysisNodeCreationOptions options,
        AnalysisNodeCreatorContainer parentContainer)
        : base(options, parentContainer)
    {
    }

    private SyntaxAnalysisNodeCreator SyntaxCreator => ParentContainer.SyntaxCreator;
    private SemanticModelAnalysisNodeCreator SemanticCreator => ParentContainer.SemanticCreator;

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case ISymbol symbol:
                return CreateRootSymbol(symbol, valueSource);

            default:
                break;
        }

        // fallback
        return SyntaxCreator.CreateRootViewNode(value, valueSource)
            ?? SemanticCreator.CreateRootViewNode(value, valueSource);
    }

    private AnalysisTreeListNode? CreateRootSymbol(ISymbol symbol, DisplayValueSource valueSource)
    {
        return null;
    }
}
