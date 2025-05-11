using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpAliasSimplifiedTypeCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : CSharpTypeCommonInlinesCreator(parentContainer) 
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(ITypeSymbol type)
    {
        var specialTypeAlias = KnownIdentifierHelpers.CSharp.GetTypeAlias(type.SpecialType);
        if (specialTypeAlias is not null)
        {
            return SingleKeywordRun(specialTypeAlias);
        }

        return base.CreateSymbolInline(type);
    }
}
