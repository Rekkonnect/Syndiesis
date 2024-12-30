using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class PreprocessingSymbolItemInlinesCreator(BaseSymbolItemInlinesCreatorContainer parentContainer)
    : BaseSymbolItemInlinesCreator<IPreprocessingSymbol>(parentContainer)
{
    protected override void AddModifierInlines(IPreprocessingSymbol symbol, GroupedRunInlineCollection inlines)
    {
    }

    protected override GroupedRunInline.IBuilder CreateSymbolInline(IPreprocessingSymbol symbol)
    {
        var run = Run(symbol.Name, ColorizationStyles.PreprocessingBrush);
        return new SingleRunInline.Builder(run);
    }
}
