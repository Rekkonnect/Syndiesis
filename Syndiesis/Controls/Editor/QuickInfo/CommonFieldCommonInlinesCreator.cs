using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonFieldCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<IFieldSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IFieldSymbol field)
    {
        return GetFieldBrush(field);
    }
}
