using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicPropertyCommonInlinesCreator(
    VisualBasicSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseVisualBasicMemberCommonInlinesCreator<IPropertySymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IPropertySymbol symbol)
    {
        return ColorizationStyles.PropertyBrush;
    }
}
