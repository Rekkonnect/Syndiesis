using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpPropertyCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<IPropertySymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IPropertySymbol symbol)
    {
        return ColorizationStyles.PropertyBrush;
    }

    protected override GroupedRunInline.IBuilder CreateSymbolInlineCore(IPropertySymbol symbol)
    {
        if (symbol.IsIndexer)
        {
            return CreateIndexerSymbolInline(symbol);
        }

        return base.CreateSymbolInlineCore(symbol);
    }

    private GroupedRunInline.IBuilder CreateIndexerSymbolInline(IPropertySymbol property)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        var thisInline = KeywordRun("this");
        inlines.AddChild(thisInline);
        var openRun = Run("[", CommonStyles.RawValueBrush);
        inlines.AddChild(openRun);
        var parameters = CreateParameterListInline(property.Parameters);
        inlines.AddNonNullChild(parameters);
        var closeRun = Run("]", CommonStyles.RawValueBrush);
        inlines.AddChild(closeRun);

        return inlines;
    }
}
