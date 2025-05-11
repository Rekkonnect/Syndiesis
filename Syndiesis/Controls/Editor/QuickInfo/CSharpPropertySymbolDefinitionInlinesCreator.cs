using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Runtime.CompilerServices;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpPropertySymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IPropertySymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IPropertySymbol property)
    {
        // TODO: Create
        return new ComplexGroupedRunInline.Builder();
    }
}