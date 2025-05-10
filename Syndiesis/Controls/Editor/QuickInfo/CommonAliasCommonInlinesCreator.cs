using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonAliasCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<IAliasSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IAliasSymbol symbol)
    {
        // TODO: Get the appropriate brush
        var brush = CommonStyles.RawValueBrush;
        var target = symbol.Target;
        var targetInline = ParentContainer.CreatorForSymbol(target).CreateSymbolInline(target);

        var builder = new ComplexGroupedRunInline.Builder();

        // TODO: Add children
        
        return builder;
    }
}