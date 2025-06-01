using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolExtraInlinesCreatorContainer
    : BaseSymbolExtraInlinesCreatorContainer
{
    private readonly CSharpTypeSymbolExtraInlinesCreator _typeSymbolTypeParameters;
    private readonly CSharpMethodSymbolExtraInlinesCreator _methodSymbolTypeParameters;
    private readonly CSharpTypeParameterSymbolExtraInlinesCreator _typeParameters;

    public CSharpSymbolExtraInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _typeSymbolTypeParameters = new(this);
        _methodSymbolTypeParameters = new(this);
        _typeParameters = new(this);
    }

    protected override ISymbolItemInlinesCreator? FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol { Arity: > 0 }:
                return _typeSymbolTypeParameters;

            case IMethodSymbol { Arity: > 0 }:
                return _methodSymbolTypeParameters;

            case ITypeParameterSymbol:
                return _typeParameters;

            default:
                return null;
        }
    }
}
