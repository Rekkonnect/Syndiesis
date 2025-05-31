using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

[Obsolete("This is a placeholder creator only meant to be used for unimplemented symbol kinds")]
internal sealed class VisualBasicFallbackSymbolDefinitionInlinesCreator(
    VisualBasicSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseVisualBasicSymbolDefinitionInlinesCreator<ISymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(ISymbol symbol)
    {
        return new SimpleGroupedRunInline.Builder();
    }
}
