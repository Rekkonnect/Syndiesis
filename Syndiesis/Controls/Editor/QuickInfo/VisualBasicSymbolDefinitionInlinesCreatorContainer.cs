using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolDefinitionInlinesCreatorContainer
    : BaseSymbolDefinitionInlinesCreatorContainer
{
    public readonly VisualBasicNamedTypeSymbolDefinitionInlinesCreator NamedTypeCreator;
    private readonly VisualBasicFallbackSymbolDefinitionInlinesCreator _fallback;

    public VisualBasicSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        NamedTypeCreator = new(this);
        _fallback = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol:
                return NamedTypeCreator;

            default:
                return _fallback;
        }
    }
}