using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpLabelSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<ILabelSymbol>(parentContainer)
{
    protected override void AddModifierInlines(
        ILabelSymbol symbol, ComplexGroupedRunInline.Builder inlines)
    {
        // Intentionally empty since these symbols accept no modifiers
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(ILabelSymbol symbol)
    {
        var explanation = Run("(label) ", CommonStyles.NullValueBrush);
        var nameRun = SingleRun(symbol.Name, ColorizationStyles.LabelBrush);
        return new ComplexGroupedRunInline.Builder([explanation, nameRun]);
    }
}
