#if ALLOW_DEV_ERRORS
using Microsoft.CodeAnalysis;
#endif
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
#if ALLOW_DEV_ERRORS
    private readonly VisualBasicNamedTypeCommonInlinesCreator _namedTypes;
#endif

    public VisualBasicSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
#if ALLOW_DEV_ERRORS
        _namedTypes = new(this);
#endif
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
#if ALLOW_DEV_ERRORS
            case INamedTypeSymbol:
                return _namedTypes;
#endif

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
