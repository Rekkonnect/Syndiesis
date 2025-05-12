using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpPropertySymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IPropertySymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IPropertySymbol property)
    {
        var type = property.Type;
        var typeInline = ParentContainer.RootContainer.Commons.CreatorForSymbol(type).CreateSymbolInline(type);
        var space = CreateSpaceSeparatorRun();
        var nameInline = CreatePropertyNameInline(property);
        return new ComplexGroupedRunInline.Builder([typeInline.AsRunOrGrouped, space, nameInline]);
    }

    private RunOrGrouped CreatePropertyNameInline(IPropertySymbol property)
    {
        if (property.IsIndexer)
        {
            return CreateIndexerPropertyNameInline(property);
        }

        return CreateOrdinaryPropertyNameInline(property);
    }
    
    private ComplexGroupedRunInline.Builder CreateIndexerPropertyNameInline(IPropertySymbol property)
    {
        var keyword = SingleKeywordRun("this");
        var rawBrush = CommonStyles.RawValueBrush;
        var openBracket = Run("[", rawBrush);
        var parameterList = CreateParameterListInline(property.Parameters) ?? new();
        var closeBracket = Run("]", rawBrush);
        return new([keyword, openBracket, parameterList, closeBracket]);
    }
    
    private static UIBuilder.Run CreateOrdinaryPropertyNameInline(IPropertySymbol property)
    {
        return Run(property.Name, ColorizationStyles.PropertyBrush);
    }
}