using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpLocalCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<ILocalSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(ILocalSymbol symbol)
    {
        if (symbol.IsConst)
            return CommonStyles.ConstantMainBrush;

        return ColorizationStyles.LocalBrush;
    }
}
