using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpLocalSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<ILocalSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(ILocalSymbol property)
    {
        // TODO: Create
        return new ComplexGroupedRunInline.Builder();
    }
}
