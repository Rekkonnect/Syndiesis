using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpDiscardSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IDiscardSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IDiscardSymbol discard)
    {
        var explanation = Run("(discard) ", CommonStyles.NullValueBrush);
        var type = discard.Type;
        var typeInline = ParentContainer.RootContainer.Commons.CreatorForSymbol(type).CreateSymbolInline(type);
        var space = CreateSpaceSeparatorRun();
        var inline = ParentContainer.RootContainer.Commons.CreatorForSymbol(discard).CreateSymbolInline(discard);
        return new ComplexGroupedRunInline.Builder([explanation, new(typeInline), space, new(inline)]);
    }
}
