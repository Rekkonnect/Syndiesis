using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicNamedTypeSymbolItemInlinesCreator(
    VisualBasicSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseVisualBasicSymbolItemInlinesCreator<INamedTypeSymbol>(parentContainer)
{
    protected override GroupedRunInline.IBuilder CreateSymbolInline(INamedTypeSymbol symbol)
    {
        return new SimpleGroupedRunInline.Builder();
    }
}
