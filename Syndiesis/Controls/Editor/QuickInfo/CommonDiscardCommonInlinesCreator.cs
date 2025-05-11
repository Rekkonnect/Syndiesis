using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonDiscardCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<IDiscardSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IDiscardSymbol symbol)
    {
        return SingleRun("_", ColorizationStyles.LocalBrush);
    }
}
