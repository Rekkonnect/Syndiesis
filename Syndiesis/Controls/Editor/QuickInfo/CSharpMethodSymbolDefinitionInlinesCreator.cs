using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IMethodSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IMethodSymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        var nameInline = ParentContainer.RootContainer.Commons
            .CreatorForSymbol(symbol).CreateSymbolInline(symbol);
        inlines.AddChild(nameInline);

        return inlines;
    }
}