using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonPreprocessingCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleNameCommonInlinesCreator<IPreprocessingSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IPreprocessingSymbol symbol)
    {
        return ColorizationStyles.PreprocessingBrush;
    }
}
