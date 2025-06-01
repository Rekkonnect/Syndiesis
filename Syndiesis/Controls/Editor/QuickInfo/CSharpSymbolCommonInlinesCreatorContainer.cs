using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
    public readonly CSharpTypeCommonInlinesCreator TypeCreator;
    public readonly CSharpAliasSimplifiedTypeCommonInlinesCreator AliasSimplifiedTypeCreator;

    private readonly CSharpMethodCommonInlinesCreator _method;
    private readonly CSharpPropertyCommonInlinesCreator _property;

    public CSharpSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        TypeCreator = new(this);
        AliasSimplifiedTypeCreator = new(this);

        _method = new(this);
        _property = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case ITypeSymbol:
                return AliasSimplifiedTypeCreator;

            case IMethodSymbol:
                return _method;

            case IPropertySymbol:
                return _property;

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
