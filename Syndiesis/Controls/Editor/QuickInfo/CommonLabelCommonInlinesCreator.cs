using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonLabelCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<ILabelSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(ILabelSymbol symbol)
    {
        return ColorizationStyles.LabelBrush;
    }
}
