using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolDefinitionInlinesCreatorContainer
    : BaseSymbolDefinitionInlinesCreatorContainer
{
    public readonly CSharpNamedTypeSymbolDefinitionInlinesCreator NamedTypeCreator;

    public CSharpSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        NamedTypeCreator = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol:
                return NamedTypeCreator;

            default:
                throw new ArgumentException("The symbol type is not supported.", nameof(symbol));
        }
    }
}
