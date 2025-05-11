using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonParameterCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<IParameterSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IParameterSymbol symbol)
    {
        return ColorizationStyles.ParameterBrush;
    }
}
