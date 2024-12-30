using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolItemInlinesCreatorContainer
{
    private readonly PreprocessingSymbolItemInlinesCreator _preprocessingCreator;

    protected BaseSymbolItemInlinesCreatorContainer()
    {
        _preprocessingCreator = new(this);
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