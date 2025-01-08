using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodSymbolExtraInlinesCreator(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseCSharpTypeParameterSymbolExtraInlinesCreator<IMethodSymbol>(parentContainer)
{
}
