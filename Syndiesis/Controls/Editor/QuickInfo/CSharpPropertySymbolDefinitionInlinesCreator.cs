using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpPropertySymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IPropertySymbol>(parentContainer)
{
    protected override ModifierInfo GetModifierInfo(IPropertySymbol symbol)
    {
        var info = base.GetModifierInfo(symbol);

        // Only display the readonly modifier if the property is a member of a struct
        if (symbol.ContainingType is { TypeKind: not TypeKind.Struct })
        {
            info = info with
            {
                Modifiers = info.Modifiers & ~MemberModifiers.ReadOnly
            };
        }

        return info;
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(IPropertySymbol property)
    {
        var type = property.Type;
        var typeInline = ParentContainer.RootContainer.Commons
            .CreatorForSymbol(type).CreateSymbolInline(type);
        var space = CreateSpaceSeparatorRun();
        var nameInline = CreatePropertyNameInline(property);
        var inlines = new ComplexGroupedRunInline.Builder(
            [typeInline.AsRunOrGrouped, space, nameInline]);
        AppendAccessors(property, inlines);
        return inlines;
    }

    private void AppendAccessors(IPropertySymbol property, ComplexGroupedRunInline.Builder inlines)
    {
        var rawBrush = CommonStyles.RawValueBrush;
        var separatorRun = Run(" ", rawBrush);
        inlines.Add(separatorRun);

        var groupedInlines = new ComplexGroupedRunInline.Builder();

        var openRun = Run("{ ", rawBrush);
        groupedInlines.Add(openRun);

        AddAccessorInlines(property.GetMethod, groupedInlines);
        AddAccessorInlines(property.SetMethod, groupedInlines);

        var closeRun = Run("}", rawBrush);
        groupedInlines.AddChild(closeRun);

        inlines.Add(groupedInlines);
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