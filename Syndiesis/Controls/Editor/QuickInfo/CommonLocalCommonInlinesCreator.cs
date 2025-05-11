using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonLocalCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<ILocalSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(ILocalSymbol local)
    {
        return GetLocalBrush(local);
    }
}
