using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpNamedTypeSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<INamedTypeSymbol>(parentContainer)
{
    protected override GroupedRunInline.IBuilder CreateSymbolInline(INamedTypeSymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        
        if (symbol.IsRecord)
        {
            AddKeywordRun("record", inlines);
        }

        switch (symbol.TypeKind)
        {
            case TypeKind.Class:
                AddKeywordRun("class", inlines);
                break;
            case TypeKind.Struct:
                AddKeywordRun("struct", inlines);
                break;
            case TypeKind.Interface:
                AddKeywordRun("interface", inlines);
                break;
            case TypeKind.Enum:
                AddKeywordRun("enum", inlines);
                break;
            case TypeKind.Delegate:
                AddKeywordRun("delegate", inlines);
                break;
        }

        return inlines;
    }
}