using Microsoft.CodeAnalysis;
using Serilog;
using Syndiesis.Controls.Inlines;
using System.Diagnostics;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseCommonMemberCommonInlinesCreator<TSymbol>(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected virtual ILazilyUpdatedBrush GetBrush(TSymbol symbol)
    {
        throw new UnreachableException("If not overridden, this method should not be reachable");
    }

    protected virtual GroupedRunInline.IBuilder CreateSymbolInlineCore(TSymbol symbol)
    {
        return SingleRun(symbol.Name, GetBrush(symbol));
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol)
    {
        var nameRun = CreateSymbolInlineCore(symbol);

        var containing = symbol.ContainingSymbol;
        var creator = ParentContainer.CreatorForSymbol(containing);
        if (creator is null)
        {
            Log.Warning(
                "Received null creator for containing symbol kind {containingKind}",
                containing?.Kind);
            return nameRun;
        }

        var containerRun = creator.CreateSymbolInline(containing);
        var qualifierRun = CreateQualifierSeparatorRun();
        return new ComplexGroupedRunInline.Builder(
            [new(containerRun), qualifierRun, new(nameRun)]);
    }
}
