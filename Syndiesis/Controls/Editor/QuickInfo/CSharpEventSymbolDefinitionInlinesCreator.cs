using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpEventSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IEventSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IEventSymbol property)
    {
        // TODO: Create
        return new ComplexGroupedRunInline.Builder();
    }
}
