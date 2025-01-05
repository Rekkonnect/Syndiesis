using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolExtraInlinesCreatorContainer
    : BaseSymbolExtraInlinesCreatorContainer
{
    private readonly VisualBasicTypeParameterConstraintListInlinesCreator _typeParameters;
    private readonly VisualBasicPreprocessingSymbolInlinesCreator _preprocessing;

    public VisualBasicSymbolExtraInlinesCreatorContainer(
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
