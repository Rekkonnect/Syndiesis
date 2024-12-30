using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public interface ISymbolItemInlinesCreator
{
    public abstract GroupedRunInlineCollection Create(ISymbol symbol);
    public abstract void Create(ISymbol symbol, GroupedRunInlineCollection inlines);
}
