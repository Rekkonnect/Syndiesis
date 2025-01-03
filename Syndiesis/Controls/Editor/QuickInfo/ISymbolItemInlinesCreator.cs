using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public interface ISymbolItemInlinesCreator
{
    public sealed GroupedRunInlineCollection Create(ISymbol symbol)
    {
        var inlines = new GroupedRunInlineCollection();
        Create(symbol, inlines);
        return inlines;
    }

    public abstract void Create(ISymbol symbol, GroupedRunInlineCollection inlines);
}

public interface ISymbolItemInfoLanguageContainer
{
    
}