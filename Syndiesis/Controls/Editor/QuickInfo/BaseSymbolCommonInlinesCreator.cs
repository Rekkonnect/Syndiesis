using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolCommonInlinesCreator<TSymbol>(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolQuickInfoInlinesCreator<TSymbol, BaseSymbolCommonInlinesCreatorContainer>(
        parentContainer)
    where TSymbol : class, ISymbol
{
    protected static SingleRunInline.Builder CreateGlobalNamespaceInline()
    {
        return SingleKeywordRun("global");
    }
}
