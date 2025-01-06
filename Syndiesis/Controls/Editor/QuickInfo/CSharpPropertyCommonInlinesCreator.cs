using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpPropertyCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<IPropertySymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IPropertySymbol symbol)
    {
        return ColorizationStyles.PropertyBrush;
    }
}
