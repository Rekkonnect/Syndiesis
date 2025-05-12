#if ALLOW_DEV_ERRORS
using Microsoft.CodeAnalysis;
#endif

namespace Syndiesis.Controls.Editor.QuickInfo;

// Shared for both C# and VB; use on both language containers
public sealed class CommonSymbolDocsInlinesCreatorContainer
    : BaseSymbolDocsInlinesCreatorContainer
{
#if ALLOW_DEV_ERRORS
    private readonly CommonDefinitionDocsInlinesCreator _definition;
    private readonly CommonMethodParameterDocsInlinesCreator _parameter;
    private readonly CommonTypeParameterDocsInlinesCreator _typeParameter;
#endif

    public CommonSymbolDocsInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
#if ALLOW_DEV_ERRORS
        _definition = new(this);
        _parameter = new(this);
        _typeParameter = new(this);
#endif
    }

    public override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
#if ALLOW_DEV_ERRORS
            case IParameterSymbol:
                return _parameter;

            case ITypeParameterSymbol:
                return _typeParameter;

            default:
                return _definition;
#else
            default:
                return null!;
#endif
        }
    }
}
