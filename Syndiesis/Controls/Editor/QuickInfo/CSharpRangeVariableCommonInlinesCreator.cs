using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpRangeVariableCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<IRangeVariableSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IRangeVariableSymbol symbol)
    {
        return ColorizationStyles.RangeVariableBrush;
    }
}
