using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDefinitionInlinesCreator<TSymbol>(
    BaseSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolDefinitionInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
    public override void Create(TSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        AddModifierInlines(method, inlines);
        base.Create(method, inlines);
    }

    protected abstract void AddModifierInlines(
        TSymbol symbol, ComplexGroupedRunInline.Builder inlines);
}
