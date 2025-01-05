using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

// Shared for both C# and VB; use on both language containers
public sealed class CommonSymbolDocsInlinesCreatorContainer
    : BaseSymbolDocsInlinesCreatorContainer
{
    private readonly CommonDefinitionDocsInlinesCreator _definition;
    private readonly CommonMethodParameterDocsInlinesCreator _parameter;
    private readonly CommonTypeParameterDocsInlinesCreator _typeParameter;

    public CommonSymbolDocsInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _definition = new(this);
        _parameter = new(this);
        _typeParameter = new(this);
    }

    public override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case IParameterSymbol:
                return _parameter;

            case ITypeParameterSymbol:
                return _typeParameter;

            default:
                return _definition;
        }
    }
}
