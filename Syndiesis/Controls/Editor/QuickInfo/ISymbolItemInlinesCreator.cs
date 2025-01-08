using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public interface ISymbolItemInlinesCreator
{
    public sealed GroupedRunInlineCollection Create(SymbolHoverContext context)
    {
        var inlines = new GroupedRunInlineCollection();
        CreateWithHoverContext(context, inlines);
        return inlines;
    }

    public abstract void CreateWithHoverContext(
        SymbolHoverContext context, GroupedRunInlineCollection inlines);

    public sealed GroupedRunInlineCollection Create(ISymbol symbol)
    {
        var inlines = new GroupedRunInlineCollection();
        Create(symbol, inlines);
        return inlines;
    }

    public abstract void Create(ISymbol symbol, GroupedRunInlineCollection inlines);

    // This is present to prevent creating a new GroupedRunInlineCollection just to create
    // the inlines of the symbol in this case
    // The GroupedRunInlineCollection methods should be removed entirely, since they serve
    // no purpose other than constructing an entire inline group instance from scratch
    // The building blocks are the grouped run inline builders, and this is the basic method
    // that should be served
    public abstract GroupedRunInline.IBuilder CreateSymbolInline(ISymbol symbol);
}
