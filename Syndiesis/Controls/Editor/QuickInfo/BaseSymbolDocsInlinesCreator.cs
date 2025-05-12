using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDocsInlinesCreator<TSymbol>(
    BaseSymbolDocsInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolDocsInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
}
