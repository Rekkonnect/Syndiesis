﻿using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonNamespaceCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<INamespaceSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(INamespaceSymbol symbol)
    {
        if (symbol.IsGlobalNamespace)
        {
            return CreateGlobalNamespaceInline();
        }
        return CreateNormalNamespaceInlines(symbol);
    }

    private static GroupedRunInline.IBuilder CreateNormalNamespaceInlines(INamespaceSymbol symbol)
    {
        var runs = new List<RunOrGrouped>();
        var identifiers = symbol.YieldNamespaceIdentifiers().Reverse().ToArray();

        for (int i = 0; i < identifiers.Length; i++)
        {
            if (runs.Count > 0)
            {
                var qualifierRun = CreateQualifierSeparatorRun();
                runs.Add(qualifierRun);
            }
            
            var identifier = identifiers[i];

            // TODO: Introduce a color style for namespaces
            var nameRun = Run(identifier, CommonStyles.RawValueBrush);
            runs.Add(nameRun);
        }

        return new ComplexGroupedRunInline.Builder(runs);
    }
}
