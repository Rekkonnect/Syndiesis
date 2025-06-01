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

    protected override ComplexGroupedRunInline.Builder CreateSymbolInlineCore(IPropertySymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        CreateNameDisplayInlines(symbol, inlines);
        return inlines;
    }

    private void CreateNameDisplayInlines(
        IPropertySymbol property, ComplexGroupedRunInline.Builder inlines)
    {
        if (property.IsIndexer)
        {
            CreateIndexerSymbolInline(property, inlines);
            return;
        }

        CreateOrdinaryPropertyNameInlines(property, inlines);
    }

    private void CreateOrdinaryPropertyNameInlines(
        IPropertySymbol property, ComplexGroupedRunInline.Builder inlines)
    {
        var single = SingleRun(property.Name, GetBrush(property));
        inlines.Add(single);
    }

    private void CreateIndexerSymbolInline(
        IPropertySymbol property, ComplexGroupedRunInline.Builder inlines)
    {
        var thisInline = KeywordRun("this");
        inlines.AddChild(thisInline);
        var openRun = Run("[", CommonStyles.RawValueBrush);
        inlines.AddChild(openRun);
        var parameters = CreateParameterListInline(property.Parameters);
        inlines.AddNonNullChild(parameters);
        var closeRun = Run("]", CommonStyles.RawValueBrush);
        inlines.AddChild(closeRun);
    }
}
