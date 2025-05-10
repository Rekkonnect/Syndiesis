using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolDefinitionInlinesCreatorContainer
    : BaseSymbolDefinitionInlinesCreatorContainer
{
    public readonly CSharpNamedTypeSymbolDefinitionInlinesCreator NamedTypeCreator;
    public readonly CSharpMethodSymbolDefinitionInlinesCreator MethodCreator;

    public CSharpSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        NamedTypeCreator = new(this);
        MethodCreator = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol:
                return NamedTypeCreator;

            case IMethodSymbol:
                return MethodCreator;

            default:
                return RootContainer.Commons.CreatorForSymbol(symbol);
        }
    }
}
