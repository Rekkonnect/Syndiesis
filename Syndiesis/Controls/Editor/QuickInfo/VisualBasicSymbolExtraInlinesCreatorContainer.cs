namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolExtraInlinesCreatorContainer(
    ISymbolInlinesRootCreatorContainer rootContainer)
        : BaseSymbolExtraInlinesCreatorContainer(rootContainer)
{
    protected override ISymbolItemInlinesCreator? FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        // No additional creators are provided
        // Type parameter constraints are placed inline in the definition inlines
        // Since they are declared in the same place as the names of the type parameters
        // themselves
        return null;
    }
}
