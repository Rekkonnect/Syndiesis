using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpTypeParameterSymbolExtraInlinesCreator(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseCSharpTypeParameterSymbolExtraInlinesCreator<ITypeParameterSymbol>(parentContainer)
{
    protected override ImmutableArray<ITypeParameterSymbol> GetTypeParameters(
        ITypeParameterSymbol symbol)
    {
        return [symbol];
    }
}
