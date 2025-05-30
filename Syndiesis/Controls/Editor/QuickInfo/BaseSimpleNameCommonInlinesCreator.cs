﻿using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSimpleNameCommonInlinesCreator<TSymbol>(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected abstract ILazilyUpdatedBrush GetBrush(TSymbol symbol);

    public sealed override GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol)
    {
        return SingleRun(symbol.Name, GetBrush(symbol));
    }
}