using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolExtraInlinesCreatorContainer
    : BaseSymbolInlinesCreatorContainer
{
    private readonly CommonPreprocessingSymbolExtraInlinesCreator _preprocessing;

    protected BaseSymbolExtraInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _preprocessing = new(this);
    }

    protected abstract ISymbolItemInlinesCreator? FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : class, ISymbol;

    public sealed override ISymbolItemInlinesCreator? CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case IPreprocessingSymbol:
                return _preprocessing;

            default:
                return FallbackCreatorForSymbol(symbol);
        }
    }
}
