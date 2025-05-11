using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpParameterSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IParameterSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IParameterSymbol property)
    {
        // TODO: Create
        return new ComplexGroupedRunInline.Builder();
    }
}
