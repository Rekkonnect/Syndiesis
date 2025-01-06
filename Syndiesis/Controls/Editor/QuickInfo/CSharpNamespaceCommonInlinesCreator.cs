using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Collections.Generic;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpNamespaceCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<INamespaceSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(INamespaceSymbol symbol)
    {
        var runs = new List<RunOrGrouped>();
        var constituent = symbol.ConstituentNamespaces;

        for (int i = 0; i < constituent.Length; i++)
        {
            var constituentNamespace = constituent[i];

            // TODO: Introduce a color style for namespaces
            var nameRun = Run(constituentNamespace.Name, CommonStyles.RawValueBrush);
            runs.Add(nameRun);

            if (i < constituent.Length - 1)
            {
                var qualifierRun = CreateQualifierSeparatorRun();
                runs.Add(qualifierRun);
            }
        }

        return new ComplexGroupedRunInline.Builder(runs);
    }
}
