using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public interface ISymbolItemInlinesCreator
{
    public sealed ComplexGroupedRunInline.Builder Create(SymbolHoverContext context)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        CreateWithHoverContext(context, inlines);
        return inlines;
    }

    public abstract void CreateWithHoverContext(
        SymbolHoverContext context, ComplexGroupedRunInline.Builder inlines);

    public sealed ComplexGroupedRunInline.Builder Create(ISymbol symbol)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        Create(symbol, inlines);
        return inlines;
    }

    public abstract void Create(ISymbol symbol, ComplexGroupedRunInline.Builder inlines);

    // This is present to enable only creating the symbol's inlines and leaving it up to the
    // caller to use the created builder; either placing it directly in the inlines, or reusing
    // it afterwards
    public abstract GroupedRunInline.IBuilder CreateSymbolInline(ISymbol symbol);
}
