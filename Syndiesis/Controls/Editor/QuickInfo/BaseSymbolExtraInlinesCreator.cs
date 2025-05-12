using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolExtraInlinesCreator<TSymbol>(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolExtraInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
}
