using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDefinitionInlinesCreatorContainer
    : BaseSymbolInlinesCreatorContainer
{
    private readonly PreprocessingSymbolDefinitionInlinesCreator _preprocessingCreator;

    protected BaseSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _preprocessingCreator = new(this);
    }

    public sealed override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
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
