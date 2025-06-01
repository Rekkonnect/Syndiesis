using Microsoft.CodeAnalysis;
using RoseLynn;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

[Obsolete("Do not use this; we need a different way to display the full definitions of these symbols")]
public abstract class BaseSimpleTypedNamedCommonInlinesCreator<TSymbol>(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected abstract ILazilyUpdatedBrush GetBrush(TSymbol symbol);

    protected virtual ITypeSymbol? GetSymbolType(TSymbol symbol)
    {
        return symbol.GetSymbolType();
    }
    
    public sealed override GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol)
    {
        var type = GetSymbolType(symbol)!;
        var typeRun = ParentContainer.CreatorForSymbol(type).CreateSymbolInline(type);
        var nameRun = SingleRun(symbol.Name, GetBrush(symbol));
        var space = CreateSpaceSeparatorRun();
        return new ComplexGroupedRunInline.Builder([new(typeRun), space, new(nameRun)]);
    }
}