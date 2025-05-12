using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class PreprocessingSymbolDefinitionInlinesCreator(
    BaseSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolDefinitionInlinesCreator<IPreprocessingSymbol>(parentContainer)
{
    protected override void AddModifierInlines(
        IPreprocessingSymbol symbol, ComplexGroupedRunInline.Builder inlines)
    {
        // Intentionally empty since these symbols accept no modifiers
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(IPreprocessingSymbol symbol)
    {
        var explanation = Run("(preprocessing) ", CommonStyles.NullValueBrush);
        var nameRun = SingleRun(symbol.Name, ColorizationStyles.PreprocessingBrush);
        return new ComplexGroupedRunInline.Builder([explanation, nameRun]);
    }
}
