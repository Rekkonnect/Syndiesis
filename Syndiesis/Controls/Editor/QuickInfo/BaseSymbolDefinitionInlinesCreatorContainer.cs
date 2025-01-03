using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDefinitionInlinesCreatorContainer
{
    private readonly PreprocessingSymbolDefinitionInlinesCreator _preprocessingCreator;

    public readonly ISymbolInlinesRootCreatorContainer RootContainer;

    protected BaseSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
    {
        _preprocessingCreator = new(this);
        RootContainer = rootContainer;
    }

    public ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : class, ISymbol
    {
        switch (symbol)
        {
            case IPreprocessingSymbol:
                return _preprocessingCreator;

            default:
                return FallbackCreatorForSymbol(symbol);
        }
    }

    protected abstract ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : class, ISymbol;
}
