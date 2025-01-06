using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpEventCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<IEventSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IEventSymbol symbol)
    {
        return ColorizationStyles.EventBrush;
    }
}
