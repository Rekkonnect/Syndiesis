using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicNamedTypeSymbolDefinitionInlinesCreator(
    VisualBasicSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseVisualBasicSymbolDefinitionInlinesCreator<INamedTypeSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(INamedTypeSymbol symbol)
    {
        return new SimpleGroupedRunInline.Builder();
    }
}
