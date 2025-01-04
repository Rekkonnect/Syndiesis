using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolInlinesCreatorContainer(
    ISymbolInlinesRootCreatorContainer rootContainer)
{
    public readonly ISymbolInlinesRootCreatorContainer RootContainer = rootContainer;

    public abstract ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : class, ISymbol;
}
