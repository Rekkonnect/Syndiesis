using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
#if ALLOW_DEV_ERRORS
    private readonly VisualBasicNamedTypeCommonInlinesCreator _type;

    private readonly VisualBasicMethodCommonInlinesCreator _method;
#endif
    private readonly VisualBasicPropertyCommonInlinesCreator _property;

    public VisualBasicSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
#if ALLOW_DEV_ERRORS
        _type = new(this);

        _method = new(this);
#endif
        _property = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
#if ALLOW_DEV_ERRORS
            case ITypeSymbol:
                return _type;

            case IMethodSymbol:
                return _method;
#endif

            case IPropertySymbol:
                return _property;

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
