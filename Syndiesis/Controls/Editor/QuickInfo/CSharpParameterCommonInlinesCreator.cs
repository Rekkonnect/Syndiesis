using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpParameterCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<IParameterSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IParameterSymbol symbol)
    {
        return ColorizationStyles.ParameterBrush;
    }
}
