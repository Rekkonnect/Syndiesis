using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
    private readonly VisualBasicNamedTypeCommonInlinesCreator _namedTypes;
    private readonly VisualBasicPreprocessingCommonInlinesCreator _preprocessing;

    public VisualBasicSymbolCommonInlinesCreatorContainer(
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
