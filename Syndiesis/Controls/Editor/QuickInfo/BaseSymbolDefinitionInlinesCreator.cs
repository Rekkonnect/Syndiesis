using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolDefinitionInlinesCreator<TSymbol>(
    BaseSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolDefinitionInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
    public override void Create(TSymbol symbol, GroupedRunInlineCollection inlines)
    {
        AddModifierInlines(symbol, inlines);
        base.Create(symbol, inlines);
    }

    protected abstract void AddModifierInlines(
        TSymbol symbol, GroupedRunInlineCollection inlines);
}
