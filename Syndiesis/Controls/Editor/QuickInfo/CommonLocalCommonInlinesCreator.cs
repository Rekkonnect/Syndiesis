using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonLocalCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleTypedNamedCommonInlinesCreator<ILocalSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(ILocalSymbol symbol)
    {
        if (symbol.IsConst)
            return CommonStyles.ConstantMainBrush;

        return ColorizationStyles.LocalBrush;
    }
}
