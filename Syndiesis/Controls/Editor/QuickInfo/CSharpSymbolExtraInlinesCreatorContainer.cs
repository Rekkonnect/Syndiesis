using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolExtraInlinesCreatorContainer
    : BaseSymbolExtraInlinesCreatorContainer
{
    private readonly CSharpTypeParameterConstraintListInlinesCreator _typeParameters;
    private readonly CSharpPreprocessingSymbolInlinesCreator _preprocessing;

    public CSharpSymbolExtraInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _typeParameters = new(this);
        _preprocessing = new(this);
    }

    public override ISymbolItemInlinesCreator? CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol { Arity: > 0 }:
            case IMethodSymbol { Arity: > 0 }:
                return _typeParameters;

            case IPreprocessingSymbol:
                return _preprocessing;

            default:
                return null;
        }
    }
}
