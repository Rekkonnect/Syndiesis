using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
    private readonly CSharpNamedTypeCommonInlinesCreator _namedTypes;
    private readonly CSharpPreprocessingCommonInlinesCreator _preprocessing;

    public CSharpSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _namedTypes = new(this);
        _preprocessing = new(this);
    }

    // TODO: More
    public override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol:
                return _namedTypes;

            case IPreprocessingSymbol:
                return _preprocessing;

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
