using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpTypeSymbolExtraInlinesCreator(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseCSharpTypeParameterSymbolExtraInlinesCreator<INamedTypeSymbol>(parentContainer)
{
}
