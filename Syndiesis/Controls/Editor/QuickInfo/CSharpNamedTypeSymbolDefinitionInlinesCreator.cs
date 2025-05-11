using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpNamedTypeSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<INamedTypeSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(INamedTypeSymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        if (symbol.IsRecord)
        {
            AddKeywordAndSpaceRun("record", inlines);
        }

        switch (symbol.TypeKind)
        {
            case TypeKind.Class:
                AddKeywordAndSpaceRun("class", inlines);
                break;
            case TypeKind.Struct:
                AddKeywordAndSpaceRun("struct", inlines);
                break;
            case TypeKind.Interface:
                AddKeywordAndSpaceRun("interface", inlines);
                break;
            case TypeKind.Enum:
                AddKeywordAndSpaceRun("enum", inlines);
                break;
            case TypeKind.Delegate:
                AddKeywordAndSpaceRun("delegate", inlines);
                break;
        }

        var commonsContainer = (CSharpSymbolCommonInlinesCreatorContainer)ParentContainer.RootContainer.Commons;
        var nameInline = commonsContainer.TypeCreator.CreateSymbolInline(symbol);
        inlines.AddChild(nameInline);

        return inlines;
    }
}