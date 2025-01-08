using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonEventCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCommonMemberCommonInlinesCreator<IEventSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IEventSymbol symbol)
    {
        return ColorizationStyles.EventBrush;
    }
}
