using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
    private readonly CSharpTypeCommonInlinesCreator _type;

    private readonly CSharpMethodCommonInlinesCreator _method;
    private readonly CSharpPropertyCommonInlinesCreator _property;

    public CSharpSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _type = new(this);

        _method = new(this);
        _property = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case ITypeSymbol:
                return _type;

            case IMethodSymbol:
                return _method;

            case IPropertySymbol:
                return _property;

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
