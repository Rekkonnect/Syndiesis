using System;
using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolItemInlinesCreatorContainer
    : BaseSymbolItemInlinesCreatorContainer
{
    public readonly VisualBasicNamedTypeSymbolItemInlinesCreator NamedTypeCreator;

    public VisualBasicSymbolItemInlinesCreatorContainer()
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