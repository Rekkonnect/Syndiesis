using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDefinitionInlinesCreator<TSymbol>(
    BaseSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolDefinitionInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
}
