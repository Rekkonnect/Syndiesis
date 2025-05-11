using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpDiscardSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IDiscardSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IDiscardSymbol property)
    {
        // TODO: Create
        return new ComplexGroupedRunInline.Builder();
    }
}
