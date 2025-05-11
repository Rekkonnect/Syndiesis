using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Linq;

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

        if (type.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            var named = type as INamedTypeSymbol;
            var underlyingNullable = named?.TypeArguments.FirstOrDefault();
            if (underlyingNullable is not null)
            {
                var nullableMarker = Run("?", CommonStyles.RawValueBrush);
                var underlyingInline = CreateSymbolInline(underlyingNullable);
                return new ComplexGroupedRunInline.Builder([underlyingInline.AsRunOrGrouped, nullableMarker]);
            }
        }

        return base.CreateSymbolInline(type);
    }
}
